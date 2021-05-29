namespace Authentication.API.Helpers
{
    public static class ResponseMessage
    {
        #region Register

        public const string RegisteredSuccessfully = "Registered successfully";
        public const string EmailAlreadyExists = "User with this email already exists";
        public const string UsernameAlreadyExists = "User with this username already exists";

        #endregion

        #region Login

        public const string LoggedInSuccessfully = "Logged in successfully";
        public const string PasswordNotMatch = "Password does not match";
        public const string UserNotExist = "User does not exist";

        #endregion
    }
}
