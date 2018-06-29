using UnityEditor;

namespace BeatThat.Properties
{
    [CustomEditor(typeof(FormatText), true)]
	public class FormatTextEditor : PropertyBindingEditor
	{
		override public void OnInspectorGUI() 
		{
			EditorGUI.BeginChangeCheck ();
			base.OnInspectorGUI ();
			if (EditorGUI.EndChangeCheck ()) {
				(this.target as FormatText).UpdateDrivenText ();
			}
		}
	}
}
