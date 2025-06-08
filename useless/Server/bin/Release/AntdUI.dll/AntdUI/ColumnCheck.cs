using System;
using System.Windows.Forms;

namespace AntdUI;

public class ColumnCheck : Column
{
	private bool _checked;

	private CheckState checkState;

	internal bool AnimationCheck;

	internal float AnimationCheckValue;

	private ITask? ThreadCheck;

	internal CheckState checkStateOld;

	public bool Checked
	{
		get
		{
			return _checked;
		}
		set
		{
			if (_checked != value)
			{
				_checked = value;
				OnCheck();
				CheckState = (CheckState)(value ? 1 : 0);
				base.PARENT?.CheckAll(base.INDEX, this, value);
			}
		}
	}

	public CheckState CheckState
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return checkState;
		}
		internal set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Invalid comparison between Unknown and I4
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (checkState == value)
			{
				return;
			}
			checkState = value;
			base.PARENT?.OnCheckedOverallChanged(this, value);
			bool flag = (int)value == 1;
			if (_checked != flag)
			{
				_checked = flag;
				OnCheck();
			}
			if ((int)value != 0)
			{
				checkStateOld = value;
				Table? pARENT = base.PARENT;
				if (pARENT != null)
				{
					((Control)pARENT).Invalidate();
				}
			}
		}
	}

	public bool AutoCheck { get; set; } = true;


	internal bool NoTitle { get; set; } = true;


	public Func<bool, object?, int, int, bool>? Call { get; set; }

	public new Func<object?, object, int, object?>? Render { get; }

	public ColumnCheck(string key)
		: base(key, "")
	{
	}

	public ColumnCheck(string key, string title)
		: base(key, title)
	{
		NoTitle = false;
	}

	private void OnCheck()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Invalid comparison between Unknown and I4
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Invalid comparison between Unknown and I4
		ThreadCheck?.Dispose();
		if (base.PARENT == null || !((Control)base.PARENT).IsHandleCreated)
		{
			return;
		}
		if (Config.Animation)
		{
			AnimationCheck = true;
			if (_checked)
			{
				ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
				{
					AnimationCheckValue = AnimationCheckValue.Calculate(0.2f);
					if (AnimationCheckValue > 1f)
					{
						AnimationCheckValue = 1f;
						return false;
					}
					((Control)base.PARENT).Invalidate();
					return true;
				}, 20, delegate
				{
					AnimationCheck = false;
					((Control)base.PARENT).Invalidate();
				});
				return;
			}
			if ((int)checkStateOld == 1 && (int)CheckState == 2)
			{
				AnimationCheck = false;
				AnimationCheckValue = 1f;
				((Control)base.PARENT).Invalidate();
				return;
			}
			ThreadCheck = new ITask((Control)(object)base.PARENT, delegate
			{
				AnimationCheckValue = AnimationCheckValue.Calculate(-0.2f);
				if (AnimationCheckValue <= 0f)
				{
					AnimationCheckValue = 0f;
					return false;
				}
				((Control)base.PARENT).Invalidate();
				return true;
			}, 20, delegate
			{
				AnimationCheck = false;
				((Control)base.PARENT).Invalidate();
			});
		}
		else
		{
			AnimationCheckValue = (_checked ? 1f : 0f);
			((Control)base.PARENT).Invalidate();
		}
	}
}
