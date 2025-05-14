using System;
using System.Linq;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public enum ContactType
	{
		Fatal,
		Reward
	}

	public class ContactComponent : MonoBehaviour
	{
		public Action<ContactType, ContactComponent> COLLISION;

		public string[] selfFatalTags;
		public bool overrideFatalTag;
		[Space]
		public bool EndEpisodeOnFatalContact;
		public bool Contacting => _contacting;

		private bool _contacting;
		private string[] fatalTagsToUse;

		public void Initialize(string[] controllerFatalTag)
		{
			if (overrideFatalTag == false)
				fatalTagsToUse = controllerFatalTag;
			else
				fatalTagsToUse = selfFatalTags;
		}

		void OnCollisionEnter(Collision collision)
		{
			if (fatalTagsToUse.Contains(collision.gameObject.tag))
			{
				_contacting = true;
				COLLISION?.Invoke(ContactType.Fatal, this);
			}
		}

		void OnCollisionExit(Collision collision)
		{
			if (fatalTagsToUse.Contains(collision.gameObject.tag))
			{
				_contacting = false;
			}
		}

		void OnTriggerEnter(Collider other)
		{
			if (fatalTagsToUse.Contains(other.gameObject.tag))
			{
				COLLISION?.Invoke(ContactType.Fatal, this);
				_contacting = true;
			}
		}
		void OnTriggerExit(Collider other)
		{
			if (fatalTagsToUse.Contains(other.gameObject.tag))
			{
				_contacting = false;
			}
		}
		public void ResetContact()
		{
			_contacting = false;
		}
	}
}