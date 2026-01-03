using System;
using System.Collections;
using System.Collections.Generic;
using Core.Configuration;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace UI.VirtualControls
{
    public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private Canvas _mobileControlsCanvas;
        [SerializeField] private float _maxRadius = 100f;
        
        private Vector2 _centerLocalPosition;
        private RectTransform _parentCanvas;
        private float _deadZone;
        
        public event Action<Vector2> OnValueChanged;

        [Inject]
        private void Construct(JsonConfigProvider configProvider)
        {
            _deadZone = configProvider.InputSettingsRef.InputDeadzone;
        }
        
        private void Awake()
        {
            _parentCanvas = _background.parent as RectTransform;
            _mobileControlsCanvas = GetComponentInParent<Canvas>();
            
            UpdateCenterPosition();
            ResetJoystick();
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            UpdateJoystick(eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            UpdateJoystick(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            ResetJoystick();
        }
        
        private void UpdateJoystick(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas,
                screenPosition,
                null,
                out Vector2 localPoint);
            
            Vector2 direction = localPoint - _centerLocalPosition;
            float distance = Mathf.Min(direction.magnitude, _maxRadius);
            
            _handle.anchoredPosition = direction.normalized * distance;
            
            float magnitude = distance / _maxRadius;
            Vector2 normalizedDirection = direction.normalized;
            
            if (magnitude < _deadZone)
            {
                normalizedDirection = Vector2.zero;
            }
            
            OnValueChanged?.Invoke(normalizedDirection);
        }
        
        private void ResetJoystick()
        {
            _handle.anchoredPosition = Vector2.zero;
            
            OnValueChanged?.Invoke(Vector2.zero);
        }
        
        private void UpdateCenterPosition()
        {
            _centerLocalPosition = CalculateCenterWithAnchors();
        }
        
        private Vector2 CalculateCenterWithAnchors()
        {
            Vector2 anchoredPos = _background.anchoredPosition;
            
            Vector2 anchorMin = _background.anchorMin;
            Vector2 anchorMax = _background.anchorMax;
            
            if (anchorMin == anchorMax && anchorMin == new Vector2(0.5f, 0.5f))
            {
                return anchoredPos;
            }
            
            RectTransform canvasRect = _mobileControlsCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.rect.size;
            
            Vector2 anchorPoint = new Vector2(
                Mathf.Lerp(-canvasSize.x / 2, canvasSize.x / 2, (anchorMin.x + anchorMax.x) / 2),
                Mathf.Lerp(-canvasSize.y / 2, canvasSize.y / 2, (anchorMin.y + anchorMax.y) / 2));
            
            Vector2 pivot = _background.pivot;
            Vector2 size = _background.rect.size;
            Vector2 pivotOffset = new Vector2((0.5f - pivot.x) * size.x, (0.5f - pivot.y) * size.y);
            
            return anchorPoint + anchoredPos + pivotOffset;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_background == null || _parentCanvas == null) return;
            
            Vector3 centerWorld = _parentCanvas.TransformPoint(_centerLocalPosition);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(centerWorld, 5f);
            
            Gizmos.color = Color.green;
            float crossSize = 10f;
            
            Gizmos.DrawLine( centerWorld + Vector3.left * crossSize, centerWorld + Vector3.right * crossSize);
            
            Gizmos.DrawLine( centerWorld + Vector3.up * crossSize, centerWorld + Vector3.down * crossSize);
            
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(centerWorld, Vector3.forward, _maxRadius);
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            
            UnityEditor.Handles.Label(centerWorld + Vector3.up * (_maxRadius + 20f),
                $"Center: {_centerLocalPosition.ToString("F0")}", style);
        }
        #endif
    }
}
