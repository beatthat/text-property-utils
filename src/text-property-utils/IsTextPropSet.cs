
namespace BeatThat
{
	/// <summary>
	/// a bool property that is TRUE when associated TextProp is non-empty
	/// </summary>
	public class IsTextPropSet : DrivenBoolProp<TextProp>
	{
		protected override bool GetValue ()
		{
			return !string.IsNullOrEmpty(this.driver.value);
		}
	}
}