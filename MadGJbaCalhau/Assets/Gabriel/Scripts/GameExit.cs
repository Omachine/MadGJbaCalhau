using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class GameExit : MonoBehaviour
{
    [Header("Configurações da Saída")]
    [Tooltip("Dificuldade que o jogador precisa de vencer para sair (Normalmente o Boss é 5)")]
    public int dificuldadeNecessaria = 5;

    [Header("Interface Visual")]
    [Tooltip("Arrasta um GameObject de texto (TextMeshPro) para mostrar os avisos")]
    public TextMeshPro textoAviso;

    private bool jogadorEstaPerto = false;

    void Start()
    {
        // Garante que o aviso começa invisível
        if (textoAviso != null)
        {
            textoAviso.text = "";
        }
    }

    void Update()
    {
        // Se o jogador estiver na zona da porta e carregar no "E"
        if (jogadorEstaPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TentarEscapar();
        }
    }

    private void TentarEscapar()
    {
        bool podeSair = false;

        // Verifica se o PlayerStats existe e se a dificuldade máxima batida é maior ou igual a 5
        if (PlayerStats.Instance != null && PlayerStats.Instance.HighestPingPongDifficulty >= dificuldadeNecessaria)
        {
            podeSair = true;
        }

        if (podeSair)
        {
            // O jogador é o Mestre das Raquetes! Pode sair!
            if (textoAviso != null) textoAviso.text = "És o verdadeiro Mestre. Adeus!";

            Debug.Log("O Mestre das Raquetes escapou com sucesso!");

            // Aqui podes carregar uma cena de créditos ou fechar o jogo
            // UnityEngine.SceneManagement.SceneManager.LoadScene("Creditos");
            // Application.Quit(); 
        }
        else
        {
            // O jogador ainda não venceu o Boss. Bloqueia a saída!
            StopAllCoroutines();
            StartCoroutine(MostrarAvisoBloqueio());
        }
    }

    private IEnumerator MostrarAvisoBloqueio()
    {
        if (textoAviso != null)
        {
            textoAviso.color = Color.red; // Fica vermelho para dar ênfase ao aviso
            textoAviso.text = "Para escapares, tens de te tornar o Mestre das Raquetes!";
        }

        // Fica no ecrã durante 3 segundos
        yield return new WaitForSeconds(3f);

        // Volta ao texto normal de interação
        if (jogadorEstaPerto && textoAviso != null)
        {
            textoAviso.color = Color.white;
            textoAviso.text = "[E] Tentar Escapar";
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = true;
            if (textoAviso != null)
            {
                textoAviso.color = Color.white;
                textoAviso.text = "[E] Tentar Escapar";
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = false;
            StopAllCoroutines(); // Pára o aviso de bloqueio se ele se for embora
            if (textoAviso != null)
            {
                textoAviso.text = ""; // Esconde o texto
            }
        }
    }
}