using UnityEngine;
using TMPro; // Necessário para o TextMeshPro
using System.Collections;

public class NPCTrashTalker : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("Arrasta o componente TextMeshPro que está por cima do NPC")]
    public TextMeshPro textMesh;

    [Header("Trash Talk Settings")]
    [Tooltip("Lista de frases que o NPC pode dizer à sorte (Adiciona as frases no Inspector!)")]
    public string[] frasesDeTrashTalk;

    [Header("Timers (Segundos)")]
    public float tempoMinimoEspera = 4f;
    public float tempoMaximoEspera = 12f;
    [Tooltip("Velocidade com que cada letra aparece (Efeito Máquina de Escrever)")]
    public float tempoPorLetra = 0.05f;
    public float tempoDeExibicao = 3f;

    private void Start()
    {
        // Verifica se associaste o TextMeshPro no Inspector
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
                string fraseEscolhida = frasesDeTrashTalk[indexRandom];

                // Prepara o TextMeshPro para o efeito de aparecer gradualmente
                textMesh.text = fraseEscolhida;
                textMesh.maxVisibleCharacters = 0;

                // 3. Efeito "Máquina de Escrever" (revela letra a letra)
                for (int i = 0; i <= fraseEscolhida.Length; i++)
                {
                    textMesh.maxVisibleCharacters = i;
                    yield return new WaitForSeconds(tempoPorLetra);
                }
            }

            // 4. Fica a exibir o texto completo durante X segundos
            yield return new WaitForSeconds(tempoDeExibicao);

            // 5. Esconde o texto e volta ao início do loop
            textMesh.text = "";
        }
    }
}