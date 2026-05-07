namespace supermario
{
    class GameManager
    {
        public bool IsGameRunning;

        public void StartGame()  => IsGameRunning = true;
        public void EndGame()    => IsGameRunning = false;
        public void ResetGame()  => IsGameRunning = false;
    }
}
