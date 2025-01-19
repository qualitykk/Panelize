using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class PanelState
{
	public Dictionary<string, object> Properties { get; set; } = new();
	public void SetProperty(string name, object value)
	{
		if(Properties.ContainsKey(name))
		{
			Properties[name] = value;
		}
		else
		{
			Properties.Add( name, value );
		}
	}
}
