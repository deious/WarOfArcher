using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Photon.Pun;
using Photon.Realtime;

public class MapDestory : MonoBehaviourPun
{
    public Texture2D _srcTexture = null;
    private Texture2D _newTexture = null;
    private SpriteRenderer _sr = null;
    private CircleCollider2D _c2d = null;

    [SerializeField] private float _worldWidth = 0f;
    [SerializeField] private float _worldHeight = 0f;
    [SerializeField] private float _vectorPivot = 0.5f;
    [SerializeField] private float _half = 0.5f;
    [SerializeField] private float _pixelPerUnit = 100f;
    [SerializeField] private int _pixelWidth = 0;
    [SerializeField] private int _pixelHeight = 0;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _newTexture = Instantiate(_srcTexture);   // 원본을 복사하여 사용하도록 함(원본 파괴를 방지)

        _newTexture.Apply(); // 새로운 텍스처를 적용함
        MakeSprite();

        _worldWidth = _sr.bounds.size.x;  // 스프라이트 월드 x 사이즈
        _worldHeight = _sr.bounds.size.y; // 스프라이트 월드 y 사이즈
        _pixelWidth = _sr.sprite.texture.width;   // 스프라이트 텍스처 x 사이즈
        _pixelHeight = _sr.sprite.texture.height; //  스프라이트 텍스처 y 사이즈
    }

    [PunRPC]
    public void MakeHole(int colliderCenterX, int colliderCenterY, int radius)
    {
        int px, nx, py, ny, distance;
        for (int i = 0; i < radius; i++)
        {
            distance = Mathf.RoundToInt(Mathf.Sqrt(radius * radius - i * i));
            for (int j = 0; j < distance; j++)
            {
                px = colliderCenterX + i;
                nx = colliderCenterX - i;
                py = colliderCenterY + j;
                ny = colliderCenterY - j;

                _newTexture.SetPixel(px, py, Color.clear);
                _newTexture.SetPixel(nx, py, Color.clear);
                _newTexture.SetPixel(px, ny, Color.clear);
                _newTexture.SetPixel(nx, ny, Color.clear);
            }
        }

        _newTexture.Apply();
        MakeSprite();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Boom"))
        {
            return;
        }

        if (TurnManager.s_instance.CurrentPlayer.IsLocal && collision.gameObject.GetComponent<CircleCollider2D>().enabled)
        {
            _c2d = collision.GetComponent<CircleCollider2D>();
            Vector2Int circleCenter = WorldToPixel(_c2d.bounds.center);
            photonView.RPC("MakeHole", RpcTarget.All, circleCenter.x, circleCenter.y, Mathf.RoundToInt(_c2d.radius * _pixelWidth / _worldWidth));
        }
    }

    private void MakeSprite()
    {
        _sr.sprite = Sprite.Create(_newTexture, new Rect(0, 0, _newTexture.width, _newTexture.height), Vector2.one * _vectorPivot, _pixelPerUnit);
    }
    private Vector2Int WorldToPixel(Vector3 pos)
    {
        Vector2Int pixelPosition = Vector2Int.zero;

        var dx = pos.x - transform.position.x;
        var dy = pos.y - transform.position.y;

        pixelPosition.x = Mathf.RoundToInt(_half * _pixelWidth + dx * (_pixelWidth / _worldWidth));
        pixelPosition.y = Mathf.RoundToInt(_half * _pixelHeight + dy * (_pixelHeight / _worldHeight));

        return pixelPosition;
    }
}
