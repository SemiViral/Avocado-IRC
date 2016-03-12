namespace Avocado.Model {
	public class ChannelUser {
		public ChannelUser(string name) {
			Name = name;

			switch (name[0]) {
				case '&':
					Access = "Admin";
					AccessLevel = 1;
					break;
				case '~':
					Access = "Founder";
					AccessLevel = 2;
					break;
				case '@':
					Access = "Operator";
					AccessLevel = 3;
					break;
				case '+':
					Access = "Voice";
					AccessLevel = 5;
					break;
				default:
					Access = "None";
					AccessLevel = 9;
					break;
			}
		}

		public int AccessLevel { get; }
		public string Access { get; }
		public string Name { get; }
	}
}