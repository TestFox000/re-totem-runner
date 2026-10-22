using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IngameVideoPlayer : MonoBehaviour
{
	private static bool isPlaying;

	private static readonly HashSet<string> skippedMovies = new HashSet<string>();

	private static IngameVideoPlayer playbackHost;

	private const float PrepareTimeout = 12f;

	private const float PlaybackStartTimeout = 5f;

	private const float PlaybackStallTimeout = 10f;

	private const float NoVisibleFrameWarningDelay = 3f;

	private const float HeartbeatInterval = 5f;

	private const int FallbackVideoWidth = 640;

	private const int FallbackVideoHeight = 480;

	private const int MaxTextureResizes = 1;

	private VideoPlayer player;

	private RenderTexture movieTexture;

	private Rect movieRect = new Rect(0f, 0f, 0f, 0f);

	private bool preparing;

	private bool hasStarted;

	private bool hasVisibleFrame;

	private bool warnedMissingFrame;

	private bool finishing;

	private int launchFrame;

	private float prepareStartedAt;

	private float playRequestedAt;

	private float lastProgressAt;

	private float lastHeartbeatAt;

	private double lastVideoTime;

	private long lastVideoFrame = -1;

	private long firstFrameReported = -1;

	private int textureResizeCount;

	public static void PlayMovie(string movieName)
	{
		if (IsPlaying())
		{
			return;
		}
		string name = (movieName ?? string.Empty).Trim();
		if (!string.Equals(name, "intro", StringComparison.OrdinalIgnoreCase) && !string.Equals(name, "intro.mp4", StringComparison.OrdinalIgnoreCase))
		{
			if (skippedMovies.Add(name))
			{
				LogWarning("skipUnknownMovie", "name=" + Sanitize(name));
			}
			return;
		}

		GameObject hostObject = new GameObject("TotemRunner Local Movie");
		DontDestroyOnLoad(hostObject);
		playbackHost = hostObject.AddComponent<IngameVideoPlayer>();
		isPlaying = true;
		playbackHost.BeginMovie();
	}

	private void BeginMovie()
	{
		launchFrame = Time.frameCount;
		prepareStartedAt = Time.realtimeSinceStartup;
		preparing = true;
		try
		{
			player = gameObject.AddComponent<VideoPlayer>();
			player.playOnAwake = false;
			player.isLooping = false;
			player.source = VideoSource.Url;
			player.url = GetLocalMovieUrl();
			// RenderTexture path: decoding goes into a texture we own, so the
			// display no longer depends on external (OES) texture sampling in a
			// scene shader, on any camera, or on a render layer.
			player.renderMode = VideoRenderMode.RenderTexture;
			player.aspectRatio = VideoAspectRatio.FitInside;
			player.audioOutputMode = VideoAudioOutputMode.Direct;
			player.controlledAudioTrackCount = 1;
			player.EnableAudioTrack(0, true);
			player.waitForFirstFrame = true;
			EnsureMovieTexture();
			Log("begin t=" + Time.realtimeSinceStartup.ToString("F3") + " url=" + Sanitize(player.url)
				+ " screen=" + Screen.width + "x" + Screen.height
				+ " api=" + SystemInfo.graphicsDeviceType
				+ " unity=" + Application.unityVersion
				+ " renderMode=" + player.renderMode
				+ " audio=" + player.audioOutputMode
				+ " targetTexture=" + DescribeTexture());
			player.prepareCompleted += OnPrepared;
			player.started += OnStarted;
			player.loopPointReached += OnFinished;
			player.errorReceived += OnError;
			player.Prepare();
			Log("prepareRequested t=" + Time.realtimeSinceStartup.ToString("F3"));
		}
		catch (Exception exception)
		{
			LogWarning("exception", "begin " + Sanitize(exception.Message));
			FinishMovie("exception");
		}
	}

	private static string GetLocalMovieUrl()
	{
		string path = Application.streamingAssetsPath.Replace('\\', '/').TrimEnd('/') + "/intro.mp4";
		// On Android, StreamingAssets is inside the APK. Preserve Unity's
		// jar:file:// URL rather than treating it as an ordinary filesystem path.
		if (path.StartsWith("jar:file://", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		Uri uri = new Uri(path, UriKind.Absolute);
		if (!uri.IsFile)
		{
			throw new InvalidOperationException("Only a local StreamingAssets movie is supported.");
		}
		return uri.AbsoluteUri;
	}

	private void EnsureMovieTexture()
	{
		int width = player != null && player.width > 0u ? (int)player.width : FallbackVideoWidth;
		int height = player != null && player.height > 0u ? (int)player.height : FallbackVideoHeight;
		if (movieTexture != null && movieTexture.width == width && movieTexture.height == height)
		{
			return;
		}
		CreateMovieTexture(width, height);
		Log("renderTexture t=" + Time.realtimeSinceStartup.ToString("F3") + " " + width + "x" + height
			+ " format=" + movieTexture.format + " created=" + movieTexture.IsCreated()
			+ " reason=prePrepare resized=false");
		UpdateDisplayRect();
	}

	// Called after prepareCompleted only. Replacing targetTexture after a
	// successful Prepare can leave the decoder bound to the old texture, so a
	// size change returns true and the caller must Prepare() again.
	private bool TryAlignMovieTexture()
	{
		if (player == null)
		{
			return false;
		}
		int width = player.width > 0u ? (int)player.width : FallbackVideoWidth;
		int height = player.height > 0u ? (int)player.height : FallbackVideoHeight;
		if (movieTexture != null && movieTexture.width == width && movieTexture.height == height)
		{
			return false;
		}
		if (textureResizeCount >= MaxTextureResizes)
		{
			LogWarning("textureResizeSkipped", "t=" + Time.realtimeSinceStartup.ToString("F3")
				+ " rt=" + DescribeTexture() + " video=" + width + "x" + height);
			return false;
		}
		textureResizeCount++;
		CreateMovieTexture(width, height);
		Log("renderTexture t=" + Time.realtimeSinceStartup.ToString("F3") + " " + width + "x" + height
			+ " format=" + movieTexture.format + " created=" + movieTexture.IsCreated()
			+ " reason=postPrepare resized=true reprepare=yes");
		UpdateDisplayRect();
		return true;
	}

	private void CreateMovieTexture(int width, int height)
	{
		ReleaseMovieTexture();
		movieTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
		movieTexture.name = "Totem Runner Movie Target";
		movieTexture.wrapMode = TextureWrapMode.Clamp;
		movieTexture.filterMode = FilterMode.Bilinear;
		movieTexture.Create();
		if (player != null)
		{
			player.targetTexture = movieTexture;
		}
	}

	private void OnPrepared(VideoPlayer source)
	{
		if (finishing || source != player)
		{
			return;
		}
		if (TryAlignMovieTexture())
		{
			// Frames are only guaranteed to reach the texture the decoder was
			// prepared with, so re-Prepare before playing into the new one.
			Log("reprepareAfterResize t=" + Time.realtimeSinceStartup.ToString("F3")
				+ " video=" + source.width + "x" + source.height
				+ " rt=" + DescribeTexture());
			try
			{
				source.Prepare();
			}
			catch (Exception exception)
			{
				LogWarning("exception", "reprepare " + Sanitize(exception.Message));
				FinishMovie("exception");
			}
			return;
		}
		preparing = false;
		playRequestedAt = Time.realtimeSinceStartup;
		lastProgressAt = playRequestedAt;
		lastHeartbeatAt = playRequestedAt;
		warnedMissingFrame = false;
		UpdateDisplayRect();
		Log("prepared t=" + Time.realtimeSinceStartup.ToString("F3")
			+ " video=" + source.width + "x" + source.height
			+ " frames=" + source.frameCount
			+ " fps=" + source.frameRate
			+ " audioTracks=" + source.audioTrackCount
			+ " audioMode=" + source.audioOutputMode
			+ " isPrepared=" + source.isPrepared
			+ " tex=" + DescribeTexture());
		try
		{
			source.Play();
			Log("playRequested t=" + Time.realtimeSinceStartup.ToString("F3"));
		}
		catch (Exception exception)
		{
			LogWarning("exception", "play " + Sanitize(exception.Message));
			FinishMovie("exception");
		}
	}

	private void OnStarted(VideoPlayer source)
	{
		if (finishing || source != player)
		{
			return;
		}
		hasStarted = true;
		lastProgressAt = Time.realtimeSinceStartup;
		Log("started t=" + Time.realtimeSinceStartup.ToString("F3") + " frame=" + source.frame
			+ " time=" + source.time.ToString("F3") + " isPlaying=" + source.isPlaying
			+ " tex=" + DescribeTexture());
	}

	private void OnFinished(VideoPlayer source)
	{
		if (finishing)
		{
			return;
		}
		Log("loopPointReached t=" + Time.realtimeSinceStartup.ToString("F3")
			+ " frame=" + (source != null ? source.frame : -1)
			+ " time=" + (source != null ? source.time.ToString("F3") : "n/a"));
		FinishMovie("finished");
	}

	private void OnError(VideoPlayer source, string message)
	{
		Debug.LogError("[IngameVideoPlayer] videoError " + Sanitize(message));
		FinishMovie("error");
	}

	private void Update()
	{
		if (playbackHost != this || finishing || !isPlaying)
		{
			return;
		}
		// Ignore the frame that requested playback, so the menu tap which
		// starts the movie cannot also skip it immediately.
		if (Time.frameCount > launchFrame && WantsToSkip())
		{
			FinishMovie("skip");
			return;
		}
		float now = Time.realtimeSinceStartup;
		if (preparing)
		{
			if (now - prepareStartedAt > PrepareTimeout)
			{
				LogWarning("prepareTimeout", "t=" + now.ToString("F3") + " isPrepared=" + (player != null && player.isPrepared));
				FinishMovie("prepareTimeout");
			}
			return;
		}
		if (player == null)
		{
			FinishMovie("playerLost");
			return;
		}
		if (!hasStarted)
		{
			if (player.isPlaying)
			{
				hasStarted = true;
				lastProgressAt = now;
			}
			else if (now - playRequestedAt > PlaybackStartTimeout)
			{
				LogWarning("startTimeout", "t=" + now.ToString("F3")
					+ " isPlaying=" + player.isPlaying
					+ " isPrepared=" + player.isPrepared
					+ " frame=" + player.frame
					+ " tex=" + DescribeTexture());
				FinishMovie("startTimeout");
				return;
			}
		}
		if (hasStarted)
		{
			if (player.time != lastVideoTime || player.frame != lastVideoFrame)
			{
				lastVideoTime = player.time;
				lastVideoFrame = player.frame;
				lastProgressAt = now;
			}
			else if (now - lastProgressAt > PlaybackStallTimeout)
			{
				LogWarning("stall", "t=" + now.ToString("F3")
					+ " frame=" + player.frame
					+ " time=" + player.time.ToString("F3")
					+ " visible=" + hasVisibleFrame
					+ " tex=" + DescribeTexture());
				FinishMovie("stall");
				return;
			}
			if (!hasVisibleFrame)
			{
				if (player.frame > 0)
				{
					hasVisibleFrame = true;
					firstFrameReported = player.frame;
					Log("firstFrame t=" + now.ToString("F3")
						+ " via=frame"
						+ " frame=" + player.frame
						+ " time=" + player.time.ToString("F3")
						+ " tex=" + DescribeTexture());
				}
				else if (player.time >= 1.0 && player.isPlaying)
				{
					// Frame counter stays 0 on some Android codecs even while
					// the clip advances; do not lose the whole clip to that.
					hasVisibleFrame = true;
					LogWarning("firstFrameByTime", "t=" + now.ToString("F3")
						+ " frame=" + player.frame
						+ " time=" + player.time.ToString("F3")
						+ " tex=" + DescribeTexture());
				}
				else if (!warnedMissingFrame && now - playRequestedAt > NoVisibleFrameWarningDelay)
				{
					warnedMissingFrame = true;
					LogWarning("noVisibleFrame", "t=" + now.ToString("F3")
						+ " elapsed=" + (now - playRequestedAt).ToString("F3")
						+ " frame=" + player.frame
						+ " time=" + player.time.ToString("F3")
						+ " isPlaying=" + player.isPlaying
						+ " tex=" + DescribeTexture());
				}
			}
			UpdateDisplayRect();
			if (now - lastHeartbeatAt >= HeartbeatInterval)
			{
				lastHeartbeatAt = now;
				Log("heartbeat t=" + now.ToString("F3")
					+ " frame=" + player.frame
					+ " time=" + player.time.ToString("F3")
					+ " isPlaying=" + player.isPlaying
					+ " visible=" + hasVisibleFrame);
			}
		}
	}

	private void OnGUI()
	{
		// hasVisibleFrame only feeds logging: some Android codecs never report
		// frame > 0, so drawing must not depend on that counter.
		if (playbackHost != this || finishing || player == null || movieTexture == null)
		{
			return;
		}
		if (!hasStarted && !hasVisibleFrame)
		{
			return;
		}
		if (Event.current == null || Event.current.type != EventType.Repaint)
		{
			return;
		}
		if (movieRect.width < 1f || movieRect.height < 1f)
		{
			UpdateDisplayRect();
		}
		// IMGUI draws after the scene cameras and after any Screen Space
		// Overlay canvas, so the movie is visible without owning a camera,
		// a layer, a scene quad or a uGUI RawImage.
		GUI.depth = -1000;
		Color previousColor = GUI.color;
		GUI.color = Color.black;
		GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
		GUI.color = previousColor;
		GUI.DrawTexture(movieRect, movieTexture, ScaleMode.StretchToFill, false);
	}

	private static bool WantsToSkip()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
		{
			return true;
		}
		for (int i = 0; i < Input.touchCount; i++)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
				return true;
			}
		}
		return false;
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			Log("appPaused");
			return;
		}
		if (playbackHost != this || finishing)
		{
			return;
		}
		// Time.realtimeSinceStartup includes time spent in the background.
		// Give decoding a fresh timeout window when the app returns.
		float now = Time.realtimeSinceStartup;
		prepareStartedAt = now;
		playRequestedAt = now;
		lastProgressAt = now;
		lastHeartbeatAt = now;
		Log("appResumed t=" + now.ToString("F3") + " isPrepared=" + (player != null && player.isPrepared)
			+ " visible=" + hasVisibleFrame);
		if (!preparing && player != null && player.isPrepared)
		{
			try
			{
				player.Play();
			}
			catch (Exception exception)
			{
				LogWarning("exception", "resume " + Sanitize(exception.Message));
				FinishMovie("exception");
			}
		}
	}

	public static bool IsPlaying()
	{
		// Include asynchronous Prepare and initial buffering. The old launchers
		// advance to the next screen as soon as this becomes false.
		return isPlaying && playbackHost != null;
	}

	public static void StopMovie()
	{
		if (playbackHost != null)
		{
			playbackHost.FinishMovie("stopMovie");
		}
		else
		{
			isPlaying = false;
		}
	}

	private void FinishMovie(string reason)
	{
		if (finishing)
		{
			return;
		}
		finishing = true;
		Log("finish reason=" + Sanitize(reason)
			+ " t=" + Time.realtimeSinceStartup.ToString("F3")
			+ " elapsed=" + (Time.realtimeSinceStartup - prepareStartedAt).ToString("F3")
			+ " visible=" + hasVisibleFrame
			+ " lastFrame=" + (player != null ? player.frame : -1)
			+ " firstFrame=" + firstFrameReported
			+ " tex=" + DescribeTexture());
		if (playbackHost == this)
		{
			playbackHost = null;
			isPlaying = false;
		}
		ReleasePlayer();
		Destroy(gameObject);
	}

	private void ReleasePlayer()
	{
		if (player != null)
		{
			player.prepareCompleted -= OnPrepared;
			player.started -= OnStarted;
			player.loopPointReached -= OnFinished;
			player.errorReceived -= OnError;
			try
			{
				player.Stop();
			}
			catch (Exception exception)
			{
				LogWarning("exception", "stop " + Sanitize(exception.Message));
			}
			player.targetTexture = null;
		}
		ReleaseMovieTexture();
	}

	private void ReleaseMovieTexture()
	{
		if (movieTexture == null)
		{
			return;
		}
		try
		{
			if (movieTexture.IsCreated())
			{
				movieTexture.Release();
			}
		}
		catch (Exception exception)
		{
			LogWarning("exception", "releaseTexture " + Sanitize(exception.Message));
		}
		Destroy(movieTexture);
		movieTexture = null;
	}

	private void UpdateDisplayRect()
	{
		int videoWidth = GetMovieWidth();
		int videoHeight = GetMovieHeight();
		float screenWidth = Mathf.Max(1f, Screen.width);
		float screenHeight = Mathf.Max(1f, Screen.height);
		// FitInside (contain) with black bars: same framing as the original
		// 640x480 intro on any device aspect ratio.
		float scale = Mathf.Min(screenWidth / videoWidth, screenHeight / videoHeight);
		float drawWidth = Mathf.Round(videoWidth * scale);
		float drawHeight = Mathf.Round(videoHeight * scale);
		movieRect = new Rect(
			Mathf.Round((screenWidth - drawWidth) * 0.5f),
			Mathf.Round((screenHeight - drawHeight) * 0.5f),
			drawWidth,
			drawHeight);
	}

	private int GetMovieWidth()
	{
		if (movieTexture != null)
		{
			return movieTexture.width;
		}
		if (player != null && player.width > 0u)
		{
			return (int)player.width;
		}
		return FallbackVideoWidth;
	}

	private int GetMovieHeight()
	{
		if (movieTexture != null)
		{
			return movieTexture.height;
		}
		if (player != null && player.height > 0u)
		{
			return (int)player.height;
		}
		return FallbackVideoHeight;
	}

	private string DescribeTexture()
	{
		if (movieTexture == null)
		{
			return "null";
		}
		if (!movieTexture.IsCreated())
		{
			return "notCreated";
		}
		return movieTexture.width + "x" + movieTexture.height;
	}

	private static string Sanitize(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}
		return value.Replace('\r', ' ').Replace('\n', ' ');
	}

	private static void Log(string message)
	{
		Debug.Log("[IngameVideoPlayer] " + message);
	}

	private static void LogWarning(string kind, string message)
	{
		Debug.LogWarning("[IngameVideoPlayer] " + kind + " " + message);
	}

	private void OnDestroy()
	{
		// Scene copies of this component do not own the persistent player.
		// Only destruction of the playback host should change global state.
		if (playbackHost == this)
		{
			playbackHost = null;
			isPlaying = false;
		}
		ReleasePlayer();
	}
}
