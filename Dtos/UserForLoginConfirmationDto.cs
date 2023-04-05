namespace DotnetAPI.Dtos
{
    public partial class UserForLoginConfirmationDto
    {
        public byte[] PasswordSalt {get; set;}
        public byte[] PasswordHash {get; set;}
        public UserForLoginConfirmationDto()
        {
            if(PasswordSalt == null)
            {
                PasswordSalt = new byte[0];
            }
            if(PasswordHash == null)
            {
                PasswordHash = new byte[0];
            }
        }
    }
}