using UnityEngine;
using TMPro; // Necessário para o TextMeshPro
using System.Collections;

public class NPCTrashTalker : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Arrasta o componente TextMeshPro que está por cima da cabeça do NPC")]
    public TextMeshPro textMesh;

    [Header("Trash Talk Settings")]
    [Tooltip("Lista de frases que o NPC pode dizer à sorte")]
    public string[] frasesDeTrashTalk = new string[]
    {
        "Vais ser esmagado no torneio!",
        "Essa tua boca de um traço só não me mete medo!",
        "Volta para o nível 1, novato!",
        "Achas que tens o que é preciso para o Boss?",
        "A minha raquete é mais rápida que a tua sombra.",
        "Estás a tremer? Eu percebo."
    };

    [Header("Timers (Segundos)")]
    public float tempoMinimoEspera = 4f;
    public float tempoMaximoEspera = 12f;
    public float tempoDeExibicao = 3f;

    private void Start()
    {
        if (textMesh != null)
        {
            textMesh.text = ""; // Garante que começa calado
            StartCoroutine(RotinaDeTrashTalk());
        }
        else
        {
            Debug.LogWarning("Aviso: Falta associar o TextMeshPro no NPC chamado " + gameObject.name);
        }
    }

    private IEnumerator RotinaDeTrashTalk()
    {
        // Loop infinito enquanto o NPC existir na Scene
        while (true)
        {
            // 1. Espera um tempo aleatório em silêncio
            float tempoEspera = Random.Range(tempoMinimoEspera, tempoMaximoEspera);
            yield return new WaitForSeconds(tempoEspera);

            // 2. Escolhe uma frase de trash talk à sorte
            if (frasesDeTrashTalk.Length > 0)
            {
                int indexRandom = Random.Range(0, frasesDeTrashTalk.Length);
                textMesh.text = frasesDeTrashTalk[indexRandom];
            }

            // 3. Fica a exibir o texto durante X segundos
            yield return new WaitForSeconds(tempoDeExibicao);

            // 4. Esconde o texto e volta ao início do loop
            textMesh.text = "";
        }
    }
}