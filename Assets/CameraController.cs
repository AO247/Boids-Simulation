using UnityEngine;

public class FlyCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Normalna prêdkoœæ poruszania siê kamery.")]
    [SerializeField] private float moveSpeed = 5.0f;

    [Tooltip("Mno¿nik prêdkoœci, gdy przytrzymujemy lewy Shift.")]
    [SerializeField] private float sprintMultiplier = 3.0f;

    [Tooltip("Mno¿nik prêdkoœci 'turbo', gdy przytrzymujemy lewy Ctrl.")]
    [SerializeField] private float turboMultiplier = 10.0f;

    [Header("Rotation Settings")]
    [Tooltip("Czu³oœæ myszy podczas rozgl¹dania siê.")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // Ustaw pocz¹tkow¹ rotacjê na podstawie aktualnej rotacji kamery
        Vector3 startRotation = transform.eulerAngles;
        rotationX = startRotation.y;
        rotationY = startRotation.x;
    }

    void Update()
    {
        // --- NOWA LOGIKA OBS£UGI KURSORA I ROZGL¥DANIA SIÊ ---

        // Sprawdzamy, czy prawy przycisk myszy jest przytrzymany
        if (Input.GetMouseButton(1)) // 1 to prawy przycisk myszy
        {
            // Jeœli tak, ukryj i zablokuj kursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // I wykonaj logikê rozgl¹dania siê
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            rotationX += mouseX;
            rotationY -= mouseY;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
        }
        else
        {
            // Jeœli prawy przycisk myszy nie jest wciœniêty, odblokuj i poka¿ kursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // ---------------------------------------------------------


        // --- LOGIKA PORUSZANIA SIÊ (KLAWIATURA) ---

        // Okreœl aktualn¹ prêdkoœæ
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentSpeed *= turboMultiplier;
        }

        float moveForward = Input.GetAxis("Vertical") * currentSpeed * Time.deltaTime;
        float moveSideways = Input.GetAxis("Horizontal") * currentSpeed * Time.deltaTime;

        transform.Translate(moveSideways, 0, moveForward);


        // --- LOGIKA RUCHU GÓRA/DÓ£ (Q/E) ---

        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * currentSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.down * currentSpeed * Time.deltaTime, Space.World);
        }
    }
}