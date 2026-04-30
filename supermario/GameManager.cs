namespace supermario
{
    class GameManager
    {
        public int CurrentLevel;
        public int Score;
        public bool IsGameRunning;

        public GameManager()
        {
            CurrentLevel = 1;
            Score = 0;
            IsGameRunning = false;
        }

        public void StartGame()
        {
            IsGameRunning = true;
            // Score and CurrentLevel are set by the caller (DoLevelSetup / ResetGame)
            // so we do not clobber them here
        }

        public void UpdateGame()
        {
        }

        public void EndGame()
        {
            IsGameRunning = false;
        }

        public void RestartLevel()
        {
        }

       

        public void LoadNextLevel()
        {
            CurrentLevel++;
        }

        public void ResetGame()
        {
            Score = 0;
            CurrentLevel = 1;
            IsGameRunning = false;
        }
    }
}
