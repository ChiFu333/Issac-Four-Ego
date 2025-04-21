using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class EntityVisual : MonoBehaviour
{
    private Entity _entity;
    public MoveEngine _moveEngine;
    public SpriteRenderer render { get; private set; }
    
    public void Init(Entity ent)
    {
        _entity = ent;
        _moveEngine = new MoveEngine(_entity.transform, transform);
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
    
    private bool isMoving;
    public async UniTask MoveTo(Vector3 target, float targetRotation = 0, Action afterComplete = null)
    {
        _moveEngine.SetTarget(target, targetRotation);
        if (isMoving)
        {
            while (isMoving) await UniTask.Yield();
            return;
        }
        
        isMoving = true;
        while(_moveEngine.CheckDistant())
        {
            _moveEngine.SmoothFollow();
            _moveEngine.FollowRotation();
            await UniTask.Yield();
        }
        _moveEngine.SetToTargetPos();
        afterComplete?.Invoke();
        isMoving = false;
    }

    public class MoveEngine
    {
        private Transform _cardTransform;
        private Transform _visualTransform;
        
        private Vector2 targetPosition;
        private float targetRotation;
        private Vector2 velocity; 
        private float smoothTime = 0.17f; 
        private float maxVelocity = 8.5f; 
        private Vector2 currentVelocity;
        private Vector3 movementDelta;
        private Vector3 rotationDelta;

        public MoveEngine(Transform t, Transform visual)
        {
            _cardTransform = t;
            _visualTransform = visual;
        }

        public void SetTarget(Vector2 targetPos, float targetRot)
        {
            targetPosition = targetPos;
            targetRotation = targetRot;
        }

        public bool CheckDistant()
        {
            return Vector2.Distance(_cardTransform.position, targetPosition) > 0.005f || velocity.magnitude > 0.005f;
        }
            
        public void SmoothFollow()
        {
            float speed = 5f;
            Vector2 newPosition = Vector2.SmoothDamp(_cardTransform.position, targetPosition, ref currentVelocity, smoothTime, maxVelocity, Time.deltaTime);
            //currentVelocity *= 0.945f;
            velocity = (newPosition - (Vector2)_cardTransform.position) / Time.deltaTime;

            if (velocity.sqrMagnitude > maxVelocity * maxVelocity)
            {
                velocity = velocity.normalized * maxVelocity;
            }

            Vector2 newPos = newPosition + velocity * (Time.deltaTime * speed);
            _cardTransform.position = new Vector3(newPos.x, newPos.y, _cardTransform.position.z);
        } 
        public void SmoothFollow2()
        {
            float followSpeed = 8;
            //Vector3 verticalOffset = (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));
            _cardTransform.position = Vector3.Lerp(_cardTransform.position, targetPosition /*+ verticalOffset*/, followSpeed * Time.deltaTime);
        }
        
        public void FollowRotation()
        {
            float rotationAmount = 20;
            float rotationSpeed = 20;

            Vector3 movement = (_cardTransform.position - (Vector3)targetPosition);
            movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
            Vector3 movementRotation = movement * rotationAmount;
            rotationDelta = Vector3.Lerp(rotationDelta , movementRotation, rotationSpeed * Time.deltaTime);
            _visualTransform.eulerAngles = new Vector3(_visualTransform.eulerAngles.x, _visualTransform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60) + targetRotation);
        }
        
        public void SetToTargetPos()
        {
            _cardTransform.position = new Vector3(targetPosition.x, targetPosition.y, _cardTransform.position.z);
            _visualTransform.eulerAngles = new Vector3(0, 0, this.targetRotation);
            velocity = Vector2.zero;
        }
    }
}
