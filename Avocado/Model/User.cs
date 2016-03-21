namespace Avocado.Model {
    public class User {
        public User(string name) {
            Name = name;

            switch (name[0]) {
                case '&':
                    Access = "Admin";
                    AccessLevel = 1;
                    Name = Name.Substring(1);
                    break;
                case '~':
                    Access = "Founder";
                    AccessLevel = 2;
                    Name = Name.Substring(1);
                    break;
                case '@':
                    Access = "Operator";
                    AccessLevel = 3;
                    Name = Name.Substring(1);
                    break;
                case '+':
                    Access = "Voice";
                    AccessLevel = 5;
                    Name = Name.Substring(1);
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