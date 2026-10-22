using UnityEngine;

public class OrderListLoop
{
	private int listSize;

	private SortableByInt[] list;

	private int first;

	private int quantity;

	private int curEnum;

	private int enumQuantity;

	public OrderListLoop(int size)
	{
		listSize = size;
		list = new SortableByInt[size];
		first = 0;
		quantity = 0;
		EndEnum();
	}

	public int GetListQuantity()
	{
		return quantity;
	}

	public SortableByInt GetFirstSortable()
	{
		if (quantity > 0)
		{
			return list[first];
		}
		return null;
	}

	public SortableByInt GetLastSortable()
	{
		if (quantity > 0)
		{
			int num = first + quantity - 1;
			if (num >= listSize)
			{
				num -= listSize;
			}
			return list[num];
		}
		return null;
	}

	public void InsertNod(SortableByInt obj)
	{
		if (quantity >= listSize)
		{
			Debug.LogWarning(string.Concat("Warning! Sorted list (size=", listSize, ") ", this, " is overflow on ", obj, " (order=", obj.GetOrder(), ")"));
			return;
		}
		int num = 0;
		int num2 = quantity - 1;
		int order = obj.GetOrder();
		int num3;
		while (num < num2 - 1)
		{
			num3 = (num + num2) / 2;
			if (GetOrderByIndex(num3) - order <= 0)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		if (num2 < 0 || GetOrderByIndex(num2) - order <= 0)
		{
			num3 = first + quantity;
			if (num3 >= listSize)
			{
				num3 -= listSize;
			}
		}
		else if (GetOrderByIndex(num) - order > 0)
		{
			if (--first < 0)
			{
				first = listSize - 1;
			}
			num3 = first;
		}
		else
		{
			int num4 = first + quantity;
			if (num4 >= listSize)
			{
				num4 -= listSize;
			}
			for (num3 = num2; num3 < quantity; num3++)
			{
				int num5 = num4 - 1;
				if (num5 < 0)
				{
					num5 = listSize - 1;
				}
				list[num4] = list[num5];
				num4 = num5;
			}
			num3 = first + num2;
			if (num3 >= listSize)
			{
				num3 -= listSize;
			}
		}
		list[num3] = obj;
		quantity++;
		if (enumQuantity < listSize)
		{
			num3 -= first;
			if (num3 < 0)
			{
				num3 += listSize;
			}
			int num6 = curEnum - first;
			if (num6 < 0)
			{
				num6 += listSize;
			}
			if (num3 <= num6)
			{
				curEnum++;
				enumQuantity++;
			}
		}
	}

	public void ShiftNod()
	{
		if (quantity > 0)
		{
			if (++first >= listSize)
			{
				first = 0;
			}
			quantity--;
			if (enumQuantity < listSize)
			{
				enumQuantity--;
			}
		}
	}

	public void PopNod()
	{
		if (quantity > 0)
		{
			quantity--;
		}
	}

	public void OnChangeOrder(SortableByInt obj)
	{
		int num = first;
		int i;
		for (i = 0; i < quantity; i++)
		{
			if (list[num] == obj)
			{
				break;
			}
			if (++num >= listSize)
			{
				num = 0;
			}
		}
		if (i >= quantity)
		{
			return;
		}
		while (num != first)
		{
			int num2 = num - 1;
			if (num2 < 0)
			{
				num2 = listSize - 1;
			}
			if (list[num].GetOrder() >= list[num2].GetOrder())
			{
				break;
			}
			SortableByInt sortableByInt = list[num];
			list[num] = list[num2];
			list[num2] = sortableByInt;
			num = num2;
		}
		int num3 = first + quantity - 1;
		if (num3 >= listSize)
		{
			num3 -= listSize;
		}
		while (num != num3)
		{
			int num4 = num + 1;
			if (num4 >= listSize)
			{
				num4 -= listSize;
			}
			if (list[num].GetOrder() <= list[num4].GetOrder())
			{
				break;
			}
			SortableByInt sortableByInt = list[num];
			list[num] = list[num4];
			list[num4] = sortableByInt;
			if (enumQuantity < listSize && num == curEnum)
			{
				curEnum--;
			}
			num = num4;
		}
	}

	public void ClearList()
	{
		quantity = 0;
	}

	public void StartEnum()
	{
		curEnum = first;
		enumQuantity = 0;
	}

	public void EndEnum()
	{
		enumQuantity = 10000;
	}

	public void NextEnum()
	{
		enumQuantity++;
		if (++curEnum >= listSize)
		{
			curEnum = 0;
		}
	}

	public bool IsHaveEnum()
	{
		return enumQuantity < quantity;
	}

	public SortableByInt GetEnum()
	{
		return list[curEnum];
	}

	private int GetOrderByIndex(int n)
	{
		n += first;
		if (n >= listSize)
		{
			n -= listSize;
		}
		return list[n].GetOrder();
	}

	public void DebugTrace()
	{
		Debug.Log(Time.time + "OrderList size=" + listSize + ", first=" + first + ", quantity=" + quantity);
		int num = first;
		for (int i = 0; i < quantity; i++)
		{
			Debug.Log(i + " sortable = " + list[num]);
			if (++num >= listSize)
			{
				num = 0;
			}
		}
		Debug.Log("Current enum = " + curEnum + ", enum quantity=" + enumQuantity);
	}
}
