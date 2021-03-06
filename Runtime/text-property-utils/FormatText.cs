using System;
using BeatThat.Pools;
using BeatThat.TransformPathExt;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace BeatThat.Properties
{
    /// <summary>
    /// Use a format string for text and plugin properties to supply the inputs.
    /// Updates target text when any of the inputs changes.
    /// </summary>
    public class FormatText : PropertyBinding<IHasText, HasText>
	{
		/// <summary>
		/// The String.format text format what will be applied to the driven text.
		/// </summary>
		[Multiline]
		public string m_format = "{0}";

		/// <summary>
		/// The orderer inputs that will be applied to the format, e.g.
		/// 
		/// If the format string is "My Name: {0} {1}", 
		/// then maybe the input array should be [ firstName, lastName ]
		/// </summary>
		[Tooltip("array of properties that will supply the inputs to the formatted text, e.g. ig the format is '{0}/{1}' then there should be 2 inputs.")]
		public HasValue[] m_inputs;

		[Tooltip("array of placeholder inputs that will fill in the format while you're editting it.")]
		public string[] m_editorPlaceholderInputs = new string[] { "placeholder" };

		[Tooltip("update the driven text with format and inputs when Bind is called")]
		public bool m_updateDrivenTextOnBind = true;

		/// <summary>
		/// If the format has multiple inputs, then better to collect all input updates for a frame and then apply on LateUpdate.
		/// </summary>
		[Tooltip("If TRUE, then waits until end of frame to update text (on frames where an input value has changed). Default is TRUE")]
		[FormerlySerializedAs("m_delayMultiInputUpdateUntilEndOfFrame")]
		public bool m_delayTextUpdateUntilEndOfFrame = true;

		[Tooltip("Enable if you want to be able to truncate args in the output using {0:$MAX_LEN}, e.g. {0:6} (arg zero with max length of 6)")]
		public bool m_enableStringLimiter;

		[Tooltip("When string limiter is enabled, add ellipses if a string is truncated?")]
		public bool m_onStringLimitAddEllipsis = true;


		public string format { get { return m_format; } set { m_format = value; } }

		#if UNITY_EDITOR
		virtual protected void Reset()
		{
//			m_driven = GetComponent<HasText>();
			m_inputs = new HasValue[1];
		}
		#endif
			
		override protected void BindProperty()
		{
			if(m_inputs == null) {
				return;
			}

			foreach(var i in m_inputs) {
				if(i != null) {
					Bind(i.onValueObjChanged, this.inputValueChangedAction);
				}
			}

			UpdateDrivenText();
		}

		private void OnInputValueChanged()
		{
			#if BT_DEBUG_UNSTRIP
			if(m_debug) {
				Debug.Log("[" + Time.frameCount + "][" + this.Path() + "] input value changed");
			}
			#endif

			this.drivenTextUpdatePending = true;
			this.enabled = true;
		}
		private UnityAction inputValueChangedAction { get { return m_inputValueChangedAction?? (m_inputValueChangedAction = this.OnInputValueChanged); } }
		private UnityAction m_inputValueChangedAction;

		/// <summary>
		/// Update the driven text with the formatted value and inputs
		/// </summary>
		public void UpdateDrivenText()
		{
			#if BT_DEBUG_UNSTRIP
			if(m_debug) {
			Debug.Log("[" + Time.frameCount + "][" + this.Path() + "] UpdateDrivenText");
			}
			#endif

			if(m_inputs == null || m_inputs.Length == 0) {
				#if BT_DEBUG_UNSTRIP
				if(m_debug) {
					Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] UpdateDrivenText no inputs set");
				}
				#endif
				this.driven.value = this.format;
				return;
			}

			using(var inputList = ListPool<object>.Get()) {
				foreach(var i in m_inputs) {
					if (i == null) {
						#if UNITY_EDITOR || DEBUG_UNSTRIP
						Debug.LogWarning("[" + Time.frameCount + "] " + this.Path() + " missing input item at index " + i);
						#endif
						inputList.Add("");
						continue;
					}

					inputList.Add(i.valueObj);
				}

				using(var inputArgs = ArrayPool<object>.Get(inputList.Count)) {
					
					if (!Application.isPlaying) {
						for (var i = 0; i < inputArgs.array.Length; i++) {
							inputArgs.array [i] = m_editorPlaceholderInputs != null && m_editorPlaceholderInputs.Length > 0 ? 
								m_editorPlaceholderInputs [Mathf.Min (m_editorPlaceholderInputs.Length - 1, i)] : "";
						}
					}
					else {
						inputList.CopyTo(inputArgs.array);
					}

					this.driven.value = string.Format(this.format, inputArgs.array);
				}
			}

		}

		virtual protected void LateUpdate()
		{
			if(this.drivenTextUpdatePending) {
				UpdateDrivenText();
				this.drivenTextUpdatePending = false;
			}
		}

		private bool drivenTextUpdatePending { get; set; }

		/// <summary>
		/// Frequently want to truncate variables in a format
		/// </summary>
		class StringLimiter : IFormatProvider, ICustomFormatter
		{
			public static readonly StringLimiter SHARED = new StringLimiter();
			public static readonly StringLimiter SHARED_ELLIPSIS = new StringLimiter(true);

			public StringLimiter(bool ellipsis = false)
			{
				this.ellipsis = ellipsis;
			}

			public bool ellipsis { get; set; }

			public object GetFormat(Type formatType)
			{
				return this;
			}

			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				string s = arg as string;
				if (s != null) {
					int length;
					if (int.TryParse(format, out length)) {
						if(s.Length <= length) {
							return s;
						}

						if(length >= 3 && this.ellipsis) {
							return string.Format("{0}...", s.Substring(0, length - 3).Trim());
						}
							
						return s.Substring (0, length).Trim();
					}
				}
				return string.Format("{0:" + format + "}", arg);
			}
		}
	}
}



