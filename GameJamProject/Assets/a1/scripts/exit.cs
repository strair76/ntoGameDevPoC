using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Этот метод должен быть public, чтобы кнопка могла его увидеть
    public void QuitGame()
    {
        Debug.Log("Выход из игры!"); // Выведет сообщение в консоль для проверки

        // Закрывает скомпилированное приложение
        Application.Quit();

        // Опционально: останавливает игру, если вы тестируете её в редакторе Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}