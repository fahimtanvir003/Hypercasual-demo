using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance { get; private set; }
    private Transform childTransForm;
    
    private void Awake()
    {
        instance = this;
    }

    public void PlayParticle(int _index, Vector3 _position)
    {
        childTransForm = transform.GetChild(_index);

        ParticleSystem part = childTransForm.GetComponent<ParticleSystem>();
        childTransForm.position = _position;
        part.Clear();
        part.Play();
    }
}