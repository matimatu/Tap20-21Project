namespace Marino.DB
{
    public static class Auxiliary
    {
        public const int HashedPwSize = 24;
        public const int SaltSize = 24;
        public const int HashedPwOccupation = (HashedPwSize + SaltSize) / 3 * 4;
        public const int IterationNumber = 100000;

    }
}
