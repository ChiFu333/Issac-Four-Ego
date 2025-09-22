using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class CubeThrower : MonoBehaviour
{
    [SerializeField] private Transform cube;
    [SerializeField] private GameObject close;
    [SerializeField] private float _fastRotationDuration = 0.5f;
    [SerializeField] private float _slowRotationDuration = 2f;
    [SerializeField] private float _fastYRotation = 360f; 
    [SerializeField] private float _slowXRotation = -90f;    
    
    private Sequence _rotationSequence;
    private int result;
    public async UniTask<int> ThrowCube()
    {
        G.Main.ActionChecker.isThrowingCube = true;
        G.LightController.SetLight(G.LightController.config.NORMAL_LIGHTOUT).Forget();
        trigger = false;
        close.SetActive(true);
        StartRotation();
        result = Random.Range(1, 7);
        await UniTask.WaitUntil(() => trigger);
        G.AudioManager.PlaySound(R.Audio.slotspawn0, 0.2f);
        await StopRotation(result);
        await UniTask.Delay(550);
        cube.transform.DOScale(Vector3.zero, _slowRotationDuration).OnComplete(() => Camera.main.transform.GetChild(0).gameObject.SetActive(false));
        G.LightController.RestoreLight().Forget();
        G.Main.ActionChecker.isThrowingCube = false;
        return result;
    }

    // Запуск циклической анимации
    public void StartRotation()
    {
        Camera.main.transform.GetChild(0).gameObject.SetActive(true);
        cube.transform.eulerAngles = Vector3.zero;
        cube.transform.DOScale(Vector3.one, _slowRotationDuration * 1.5f).From(Vector3.zero);
        _rotationSequence?.Kill();
        _rotationSequence = DOTween.Sequence();
        _rotationSequence.Append(
            cube.transform.DORotate(
                new Vector3(_slowXRotation, _fastYRotation, 0), 
                _fastRotationDuration, 
                RotateMode.LocalAxisAdd // Вращение относительно локальной оси
            ).SetEase(Ease.Linear)
        );
        _rotationSequence.SetLoops(-1, LoopType.Restart);
    }
    
    public async UniTask StopRotation(int c)
    {
        if (!cube.gameObject.activeSelf) return;
        close.SetActive(false);
        _rotationSequence?.Kill();
        
        await cube.transform.DORotate(GetRotationFromInt(c) + new Vector3(360,360), _slowRotationDuration, RotateMode.FastBeyond360).AsyncWaitForCompletion().AsUniTask(); // Плавный сброс
         
        await UniTask.Delay(200);
        cube.transform.DOScale(Vector3.one * 1.8f, _slowRotationDuration * 1f).SetEase(Ease.OutQuad);
    }

    private Vector3 GetRotationFromInt(int i)
    {
        if (i == 1)
        {
            return new Vector3(0, 180, 0);
        }
        if (i == 2)
        {
            return new Vector3(-90, 0, 0);
        }
        if (i == 3)
        {
            return new Vector3(0, -90, 0);
        }
        if (i == 4)
        {
            return new Vector3(0, 90, 0);
        }
        if (i == 5)
        {
            return new Vector3(90, 0, 0);
        }
        if (i == 6)
        {
            return new Vector3(0, 0, 0);
        }

        return Vector3.negativeInfinity;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            result = 1;
            trigger = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            result = 2;
            trigger = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            result = 3;
            trigger = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            result = 4;
            trigger = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            result = 5;
            trigger = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            result = 6;
            trigger = true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            ThrowCube();
        }
    }

    private bool trigger;
    public void AgreeButton()
    {
        trigger = true;
    }
}
