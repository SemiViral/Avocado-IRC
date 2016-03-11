using System.Security.AccessControl;

namespace Avocado.Model {
	public class ChannelUser {
		public char Access { get; }
		public string Name { get; }

		public ChannelUser(char access, string name) {
			Access = access;	
			Name = name;
		}
	}
}
