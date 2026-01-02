using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.VirtualControls
{
    public class VirtualJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private Canvas _mobileControlsCanvas;
        [SerializeField] private float _maxRadius = 100f;
        [SerializeField] private float _deadZone = 0.2f;
        
        private Vector2 _centerLocalPosition;
        private RectTransform _parentCanvas;
        
        public event Action<Vector2, float> OnValueChanged;
        
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
                out Vector2 localPoint
            );
            
            Vector2 direction = localPoint - _centerLocalPosition;
            float distance = Mathf.Min(direction.magnitude, _maxRadius);
            
            _handle.anchoredPosition = direction.normalized * distance;
            
            float magnitude = distance / _maxRadius;
            Vector2 normalizedDirection = direction.normalized;
            
            if (magnitude < _deadZone)
            {
                normalizedDirection = Vector2.zero;
                magnitude = 0;
            }
            
            OnValueChanged?.Invoke(normalizedDirection, magnitude);
        }
        
        private void ResetJoystick()
        {
            _handle.anchoredPosition = Vector2.zero;
            
            OnValueChanged?.Invoke(Vector2.zero, 0);
        }
        
        private void UpdateCenterPosition()
        {
            // Automatic calculation of the center taking into account anchors and Reference Resolution
            _centerLocalPosition = CalculateCenterWithAnchors();
        }
        
        private Vector2 CalculateCenterWithAnchors()
        {
            // Getting the background's anchoredPosition
            Vector2 anchoredPos = _background.anchoredPosition;
            
            // Getting anchorMin and anchorMax
            Vector2 anchorMin = _background.anchorMin;
            Vector2 anchorMax = _background.anchorMax;
            
            // If the anchors are in the center (0.5, 0.5) - use anchoredPosition as is
            if (anchorMin == anchorMax && anchorMin == new Vector2(0.5f, 0.5f))
            {
                return anchoredPos;
            }
            
            // Get the dimensions of the parent Canvas
            RectTransform canvasRect = _mobileControlsCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.rect.size;
            
            // Calculate the anchor point position in local Canvas coordinates
            // anchorMin and anchorMax in the range [0,1] relative to the Canvas dimensions
            Vector2 anchorPoint = new Vector2(
                Mathf.Lerp(-canvasSize.x / 2, canvasSize.x / 2, (anchorMin.x + anchorMax.x) / 2),
                Mathf.Lerp(-canvasSize.y / 2, canvasSize.y / 2, (anchorMin.y + anchorMax.y) / 2)
            );
            
            // Correction for pivot if not in center
            Vector2 pivot = _background.pivot;
            Vector2 size = _background.rect.size;
            Vector2 pivotOffset = new Vector2(
                (0.5f - pivot.x) * size.x,
                (0.5f - pivot.y) * size.y
            );
            
            // Final center: anchor position + anchoredPosition + pivot correction
            return anchorPoint + anchoredPos + pivotOffset;
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_background == null)
                _background = GetComponent<RectTransform>();
            
            if (_handle == null && transform.childCount > 0)
                _handle = transform.GetChild(0) as RectTransform;
            
            if (Application.isPlaying)
            {
                UpdateCenterPosition();
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_background == null || _parentCanvas == null) return;
            
            // Converting the local position of the center to world coordinates
            Vector3 centerWorld = _parentCanvas.TransformPoint(_centerLocalPosition);
            
            // Draw the center point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(centerWorld, 5f);
            
            // Draw a cross in the center
            Gizmos.color = Color.green;
            float crossSize = 10f;
            Gizmos.DrawLine(
                centerWorld + Vector3.left * crossSize,
                centerWorld + Vector3.right * crossSize
            );
            Gizmos.DrawLine(
                centerWorld + Vector3.up * crossSize,
                centerWorld + Vector3.down * crossSize
            );
            
            // Draw the radius
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(centerWorld, Vector3.forward, _maxRadius);
            
            // Displaying information
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            
            UnityEditor.Handles.Label(
                centerWorld + Vector3.up * (_maxRadius + 20f),
                $"Center: {_centerLocalPosition.ToString("F0")}",
                style
            );
        }
        
        [ContextMenu("Debug Anchor Info")]
        private void DebugAnchorInfo()
        {
            Debug.Log("=== Anchor Debug Info ===");
            Debug.Log($"Background anchoredPosition: {_background.anchoredPosition}");
            Debug.Log($"Background anchorMin: {_background.anchorMin}");
            Debug.Log($"Background anchorMax: {_background.anchorMax}");
            Debug.Log($"Background pivot: {_background.pivot}");
            Debug.Log($"Background size: {_background.rect.size}");
            
            if (_mobileControlsCanvas != null)
            {
                RectTransform canvasRect = _mobileControlsCanvas.GetComponent<RectTransform>();
                Debug.Log($"Canvas size: {canvasRect.rect.size}");
                Debug.Log($"Canvas reference resolution: " +
                          $"{_mobileControlsCanvas.GetComponent<CanvasScaler>()?.referenceResolution}");
            }
            
            Vector2 calculatedCenter = CalculateCenterWithAnchors();
            Debug.Log($"Calculated center: {calculatedCenter}");
        }
        #endif
    }
}
