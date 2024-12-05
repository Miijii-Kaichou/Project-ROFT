using UnityEngine;
using UnityEngine.UI;
public class KeyControls : MonoBehaviour
{
    [SerializeField]
    private ObjectPooler _objectPooler;

    [SerializeField]
    private KeyId _keyId;

    [SerializeField]
    private PulseEffect _pulseEffect;

    [SerializeField]
    private KeyHolding _keyHolding;

    [SerializeField]
    private Image _graphics;

    public ObjectPooler GetObjectPooler() => _objectPooler;
    public KeyId GetKeyId() => _keyId;
    public PulseEffect GetPulseEffect() => _pulseEffect;
    public KeyHolding GetKeyHolding() => _keyHolding;
    public Image GetGraphics() => _graphics;
}
