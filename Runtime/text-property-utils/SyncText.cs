using BeatThat.TransformPathExt;
using BeatThat.Properties;
using UnityEngine;
using UnityEngine.Events;

namespace BeatThat.Properties
{
	public class SyncText : MonoBehaviour, Syncable
	{
		[Tooltip("by default you may leave 'syncFromText' property null and it will be searched in parent components. Set TRUE to disable")]
		public bool m_disableAutoFind;
		
		public TextProp m_syncFromText;
		private TextProp syncFromText 
		{ 
			get { 
				if(!m_disableAutoFind && m_syncFromText == null && this.transform.parent != null) {
					m_syncFromText = this.transform.parent.GetComponentInParent<TextProp>();
				}
				return m_syncFromText; 
			} 
		}

		private HasText m_toText;
		private HasText toText 
		{
			get {
				if(m_toText == null) {
					m_toText = this.GetRootDriver<HasText>();
				}
				return m_toText;
			}
		}

		private UnityEvent<string> syncEvent { get; set; }

		void OnEnable()
		{
			var syncFrom = this.syncFromText;
			if(syncFrom == null) {
				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] sync from text is null!");
				return;
			}

			Sync();

			this.syncEvent = syncFrom.onValueChanged;
			this.syncEvent.AddListener(this.syncAction);
		}

		void OnDisable()
		{
			if(this.syncEvent != null) {
				this.syncEvent.RemoveListener(this.syncAction);
				this.syncEvent = null;
			}
		}
		
//		// Update is called once per frame
//		void LateUpdate () 
//		{
//			Sync();
//		}

		public void Sync()
		{
			var toT = this.toText;
			var fromT = this.syncFromText;

			if(toT == null) {
				#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] " + GetType() + " toText target is null");
				#endif
				return;
			}

			if(fromT == null) {
				#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
				Debug.LogWarning("[" + Time.frameCount + "][" + this.Path() + "] " + GetType() + " fromText source is null");
				#endif
				return;
			}

			toT.value = fromT.value;
		}

		private void OnSync(string s)
		{
			Sync();
		}
		private UnityAction<string> syncAction { get { return m_syncAction?? (m_syncAction = this.OnSync); } }
		private UnityAction<string> m_syncAction;
	}
}


