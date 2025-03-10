using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using JetBrains.Annotations;
public class EntityVisual : MonoBehaviour
{
    public Entity entity;
    public SpriteRenderer render { get; private set; }
    private Transform cardTransform;
    public void Init(Entity ent)
    {
        entity = ent;
        cardTransform = entity.transform;
        render = GetComponent<SpriteRenderer>();
    }
    public List<Material> GetAllMaterialsInChildren()
    {
        List<Material> materials = new List<Material>();
        Queue<GameObject> queue = new Queue<GameObject>();
        queue.Enqueue(gameObject);

        while (queue.Count > 0)
        {
            GameObject current = queue.Dequeue();

            Renderer renderer = current.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                materials.Add(renderer.material);
            }

            foreach (Transform child in current.transform)
            {
                queue.Enqueue(child.gameObject);
            }
        }

        return materials;
    }
    public Vector3 targetPosition;
    private float targetRotation;
    private Vector2 velocity; 
    public float smoothTime = 0.005f; 
    public float maxVelocity = 0.001f; 
    private Vector2 currentVelocity;
    private bool isMoving;
    public bool isShine = false;
    public async Task MoveTo(Vector3 target, float targetRotation = 0, Action afterComplete = null)
    {
        targetPosition = target;
        this.targetRotation = targetRotation;
        if(isMoving) return;
        isMoving = true;
        while(Vector2.Distance(cardTransform.position, targetPosition) > 0.005f || velocity.magnitude > 0.005f)
        {
            HandPositioning();
            SmoothFollow();
            FollowRotation();
            await Task.Yield();
        }
        cardTransform.position = new Vector3(targetPosition.x, targetPosition.y, cardTransform.position.z);
        transform.eulerAngles = new Vector3(0, 0, this.targetRotation);
        velocity = Vector2.zero;
        
        afterComplete.Invoke();
        isMoving = false;
    }
    private void SmoothFollow2()
    {
        float followSpeed = 8;
        //Vector3 verticalOffset = (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));
        cardTransform.position = Vector3.Lerp(cardTransform.position, targetPosition /*+ verticalOffset*/, followSpeed * Time.deltaTime);
    }
    private Vector3 movementDelta;
    private Vector3 rotationDelta;
    private void FollowRotation()
    {
        float rotationAmount = 20;
        float rotationSpeed = 20;

        Vector3 movement = (cardTransform.position - targetPosition);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (entity.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta , movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60) + targetRotation);
    }
    private void SmoothFollow()
    {
        float speed = 3f;
        Vector2 newPosition = Vector2.SmoothDamp(cardTransform.position, targetPosition, ref currentVelocity, smoothTime, maxVelocity, Time.deltaTime);
        currentVelocity *= 0.945f;
        velocity = (newPosition - (Vector2)cardTransform.position) / Time.deltaTime;

        if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
        {
            velocity = velocity.normalized * maxVelocity;
        }

        cardTransform.position = newPosition + velocity * Time.deltaTime * speed;
        if (Vector2.Distance((Vector2)transform.position, targetPosition) < 0.01f && velocity.magnitude < 0.01f)
        {
            cardTransform.position = new Vector3(targetPosition.x, targetPosition.y, cardTransform.position.z);
            velocity = Vector2.zero;
        }
    } 
    private AnimationCurve positioning = new AnimationCurve(
        new Keyframe(0f, 0f), 
        new Keyframe(0.08333334f, 0.4043479f), 
        new Keyframe(0.1666667f, 0.6431358f), 
        new Keyframe(0.25f, 0.8037757f), 
        new Keyframe(0.3333333f, 0.9118787f), 
        new Keyframe(0.4166667f, 0.9769853f), 
        new Keyframe(0.5f, 1f), 
        new Keyframe(0.5833333f, 0.9767235f), 
        new Keyframe(0.6666667f, 0.9109904f), 
        new Keyframe(0.75f, 0.8020976f), 
        new Keyframe(0.8333333f, 0.6407319f), 
        new Keyframe(0.9166667f, 0.4017494f), 
        new Keyframe(1f, 0f)
    );
    private float positioningInfluence = .035f;
    private AnimationCurve rotation = new AnimationCurve(
        new Keyframe(0f, 1f), // Время 0, значение 10
        new Keyframe(1f, -1f) // Время 0.33, значение 0.5
    );
    private float rotationInfluence = 0.25f;
    private float curveYOffset;
    private float curveRotationOffset;
    private void HandPositioning()
    {
        curveYOffset = (positioning.Evaluate(entity.NormalizedPosition()) * positioningInfluence) * entity.SiblingAmount();
        curveYOffset = entity.SiblingAmount() < 5 ? 0 : curveYOffset;

        curveRotationOffset = rotation.Evaluate(entity.NormalizedPosition());
        transform.localPosition = new Vector2(transform.localPosition.x, curveYOffset);
    }
    public float GetAngleInHand()
    {
        float t = rotation.Evaluate(entity.NormalizedPosition()) * (rotationInfluence * entity.SiblingAmount());
        if(float.IsNaN(t)) t = 0;
        return t;
    }
}
