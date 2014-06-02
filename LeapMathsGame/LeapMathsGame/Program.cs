using System;

namespace LeapMathsGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (MathGame game = new MathGame())
            {
                game.Run();
            }
        }
    }
#endif
}

