cd "D:/User/Desktop/UnityAndroidTest"
git rm --cached -r --quiet TotemRunner/UserSettings
git add -A
git commit --amend -m "Initial commit: TotemRunner Unity 2021 Android project"
echo "--- log ---"
git log --oneline -1
echo "--- dirty files ---"
git status --short | wc -l
echo "--- tracked count ---"
git ls-files | wc -l
echo "--- tracked size ---"
git ls-files -z | while IFS= read -r -d "" f; do stat -c %s "$f"; done | awk '{s+=$1} END {printf "%.1f MB\n", s/1048576}'