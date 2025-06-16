using System.Diagnostics;
using UnityEngine;

public class TopDownCarController : MonoBehaviour
{

    [Header("Car Settings")]
    public float accelerationFactor = 30.0f;
    public float turnFactor = 10.5f;
    public float maxSpeed = 20f;
    public float driftFactor = 0.95f;

    public float reverseSpeedFactor = 0.5f;
    public float dragWhenNoInput = 3f;

    [Header("Collision Settings")]
    public float minCollisionForce = 2f; // Fuerza mínima para que ocurra deformación
    public float deformationAmount = 0.1f; // Cantidad de reducción de escala
    public float deformationRecovery = 0.01f; // Recuperación por segundo
    public float maxDeformation = 0.5f; // Máxima reducción permitida


    // Local variables
    float accelerationInput = 0;
    float steeringInput = 0;
    float rotationAngle = 0;
    float velocityVsUp = 0;
    Vector3 originalScale;
    Vector3 targetScale;

    // Components
    Rigidbody2D carRigidbody2D;

    void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void FixedUpdate()
    {
        ApplyEngineForce();
        ApplySteering();
        KillOrthogonalVelocity();
        RecoverDeformation();
        
    }
    

    void RecoverDeformation()
    {
        // Recuperación gradual de la escala original
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, deformationRecovery);
        }
    }
    public void SetInputVector(Vector2 inputVector)
    {
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

    void ApplyEngineForce()
    {
        // Calcular cuánto "adelante" estamos moviéndonos
        velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

        // Limitar velocidad máxima hacia adelante
        if (velocityVsUp > maxSpeed && accelerationInput > 0)
            return;

        // Limitar velocidad máxima en reversa
        if (velocityVsUp < -maxSpeed * reverseSpeedFactor && accelerationInput < 0)
            return;

        // Limitar velocidad máxima en cualquier dirección
        if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput != 0)
            return;

        // Aplicar arrastre si no hay entrada de aceleración
        if (accelerationInput == 0)
        {
            carRigidbody2D.linearDamping = Mathf.Lerp(carRigidbody2D.linearDamping, dragWhenNoInput, Time.fixedDeltaTime * 3);
        }
        else
        {
            carRigidbody2D.linearDamping = 0;
        }

        // Crear fuerza para el motor
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;

        // Aplicar fuerza
        carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
    }

    void ApplySteering()
    {
        // Calcular ángulo mínimo de giro según la velocidad
        float minSpeedBeforeAllowTurningFactor = carRigidbody2D.linearVelocity.magnitude / 8;
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        // Actualizar ángulo de rotación basado en input
        rotationAngle -= steeringInput * turnFactor * minSpeedBeforeAllowTurningFactor;

        // Aplicar dirección rotando el objeto del coche
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    void KillOrthogonalVelocity()
    {
        // Reducir la deriva lateral
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        carRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si es un objeto de cambio de tamaño
        if (collision.gameObject.CompareTag("Agrandar"))
        {
            AgrandarCoche();
            MejorarParametros1PorCiento();
            //Destroy(collision.gameObject); // Destruir el objeto después de usarlo
            return;
        }

        else if (collision.gameObject.CompareTag("Chiquitolina"))
        {
            EncogerCoche();
            ReducirParametros1PorCiento();
            //Destroy(collision.gameObject); // Destruir el objeto después de usarlo
            return;
        }

    }

    // Método para agrandar el coche
    private void AgrandarCoche()
    {
        // Guardar la escala actual como nueva escala base
        originalScale = transform.localScale;

        // Aplicar aumento de escala (15% más grande)
        targetScale = originalScale * 1.15f;

        // Limitar tamaño máximo (200% del tamaño original)
        targetScale.x = Mathf.Min(targetScale.x, originalScale.x * 2f);
        targetScale.y = Mathf.Min(targetScale.y, originalScale.y * 2f);

    }

    // Método para encoger el coche
    private void EncogerCoche()
    {
        // Guardar la escala actual como nueva escala base
        originalScale = transform.localScale;

        // Aplicar reducción de escala (15% más pequeño)
        targetScale = originalScale * 0.85f;

        // Limitar tamaño mínimo (50% del tamaño original)
        targetScale.x = Mathf.Max(targetScale.x, originalScale.x * 0.5f);
        targetScale.y = Mathf.Max(targetScale.y, originalScale.y * 0.5f);

    }

    // Aumenta todos los parámetros un 1% 
    public void MejorarParametros1PorCiento()
    {
        // Factores de aumento (1% = 1.01f)
        const float factor = 1.01f;
        const float factorReducido = 1.005f; // Para parámetros sensibles

        accelerationFactor *= factor;
        turnFactor *= factorReducido; // Aumento más pequeño en giro para evitar sobresteering
        maxSpeed *= factor;

        // driftFactor con límite máximo (nunca superar 0.99)
        driftFactor = Mathf.Min(driftFactor * 1.005f, 0.99f);

        reverseSpeedFactor *= factor;
        dragWhenNoInput *= 0.99f; // Reducción del 1% para menos fricción


    }

    // Disminuye todos los parámetros un 1%
    public void ReducirParametros1PorCiento()
    {
        // Factores de reducción (1% = 0.99f)
        const float factor = 0.99f;
        const float factorAumentado = 0.995f; // Para parámetros sensibles

        accelerationFactor *= factor;
        turnFactor *= factorAumentado; // Reducción más pequeña en giro

        // driftFactor con límite mínimo (nunca menor a 0.7)
        driftFactor = Mathf.Max(driftFactor * 0.995f, 0.7f);

        reverseSpeedFactor *= factor;
        dragWhenNoInput *= 1.01f; // Aumento del 1% en fricción

    }
}