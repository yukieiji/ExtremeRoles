using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExtremeRoles.Module.CustomMonoBehaviour
{

	public class ExtremeConsole : MonoBehaviour
	{
		public interface IBehavior
		{

		}

		public IBehavior Behavior { get; set; }
	}
}
