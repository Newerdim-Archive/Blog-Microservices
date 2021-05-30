namespace Authentication.API.Helpers
{
    public static class ResponseMessage
    {
        #region Register

        public const string RegisteredSuccessfully = "Registered successfully";
        public const string EmailAlreadyExists = "User with this email already exists";
        public const string UsernameAlreadyExists = "User with this username already exists";

        #endregion Register

        #region Login

        public const string LoggedInSuccessfully = "Logged in successfully";
        public const string PasswordNotMatch = "Password does not match";
        public const string UserNotExist = "User does not exist";

        #endregion Login

        #region RefreshTokens

        public const string RefreshTokenIsNullOrEmpty = "Refresh token is empty or does not exists";
        public const string RefreshTokenIsInvalid = "Refresh token is invalid";
        public const string TokensRefreshedSuccessfully = "Tokens refreshed successfully";

        #endregion
    }
}