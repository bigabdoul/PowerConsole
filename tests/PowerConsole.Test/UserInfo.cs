namespace PowerConsole.Test
{
    class UserInfo
    {
        public string FullName { get; set; }
        public int Age { get; set; }
        public string BirthCountry { get; set; }
        public string PreferredColor { get; set; }

        public override string ToString()
        {
            return $"Full name: {FullName}\nAge: {Age}\nCountry of birth: {BirthCountry}\nPreferred color: {PreferredColor}";
        }
    }
}
