using UnityEngine;

public class DashEffect : MonoBehaviour {

    [SerializeField] private float effectLifeTime;
    
    private SpriteRenderer spriteRenderer;
    private GameObject playerObject;

    private Color defaultColor;
    private float effectTimer;

    private void Awake() {

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerObject = FindObjectOfType<PlayerController>().gameObject;

        WarningMessage();

        effectTimer = effectLifeTime;
        defaultColor = spriteRenderer.color;
    }

    private void Update() {

        bool playerInFrontOfEffect;

        if (playerObject.transform.right.x == 1) {

            playerInFrontOfEffect = playerObject.transform.position.x >= transform.position.x;
        }
        else {

            playerInFrontOfEffect = playerObject.transform.position.x <= transform.position.x;
        }

        if (playerInFrontOfEffect) {

            spriteRenderer.enabled = true;
        }

        if (spriteRenderer.enabled == true) {

            effectTimer -= Time.deltaTime;
            defaultColor.a = (effectTimer / effectLifeTime) * defaultColor.a;

            if (defaultColor.a <= 0) {

                Destroy(gameObject);
            }
        }
    }

    private void WarningMessage() {

        if (spriteRenderer.enabled == true) {

            Debug.LogWarning("Dash Effect: Sprite Renderer is enabled by default. This may cause some visual bugs");
            Debug.Log("Please make sure to disable Sprite Renderer by default on this game object");
        }
    }
}
