public static class GuidConverter
{
    public static Guid ConvertToGuid(string input)
    {
        Guid guid;
        bool isValidGuid = Guid.TryParse(input, out guid);

        if (isValidGuid)
        {
            return guid;
        }
        else
        {
            throw new ArgumentException("Invalid GUID format. Unable to convert the string to GUID.");
        }
    }
}