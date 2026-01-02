using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.VirtualControls
{
    public class MobileJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _maxRadius = 100f;
        
        [Header("Anchor Correction")]
        [SerializeField] private bool _useManualCenter = false;
        [SerializeField] private Vector2 _manualCenterLocal = Vector2.zero;
        
        private Vector2 _centerLocalPosition;
        private RectTransform _parentCanvas;
        
        public event System.Action<Vector2, float> OnValueChanged;
        
        private void Awake()
        {
            _parentCanvas = _background.parent as RectTransform;
            UpdateCenterPosition();
            ResetJoystick();
        }
        
        private void UpdateCenterPosition()
        {
            if (_useManualCenter)
            {
                _centerLocalPosition = _manualCenterLocal;
            }
            else
            {
                // Автоматический расчёт центра
                _centerLocalPosition = GetAutoCalculatedCenter();
            }
        }
        
        private Vector2 GetAutoCalculatedCenter()
        {
            // anchoredPosition с учётом pivot
            Vector2 anchoredPos = _background.anchoredPosition;
            Vector2 pivot = _background.pivot;
            Vector2 size = _background.rect.size;
            
            // Корректировка для pivot не в центре
            Vector2 pivotOffset = new Vector2(
                (0.5f - pivot.x) * size.x,
                (0.5f - pivot.y) * size.y
            );
            
            return anchoredPos + pivotOffset;
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
            
            OnValueChanged?.Invoke(normalizedDirection, magnitude);
        }
        
        private void ResetJoystick()
        {
            _handle.anchoredPosition = Vector2.zero;
            OnValueChanged?.Invoke(Vector2.zero, 0);
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (_background == null)
                _background = GetComponent<RectTransform>();
            
            if (_handle == null && transform.childCount > 0)
                _handle = transform.GetChild(0) as RectTransform;
            
            UpdateCenterPosition();
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_background == null || _parentCanvas == null) return;
            
            // Конвертируем локальную позицию центра в мировые координаты
            Vector3 centerWorld = _parentCanvas.TransformPoint(_centerLocalPosition);
            
            // Рисуем точку центра
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(centerWorld, 5f);
            
            // Рисуем крестик в центре
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
            
            // Рисуем радиус
            UnityEditor.Handles.color = Color.cyan;
            UnityEditor.Handles.DrawWireDisc(centerWorld, Vector3.forward, _maxRadius);
            
            // Рисуем оси для ориентации
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawLine(
                centerWorld,
                centerWorld + _parentCanvas.right * _maxRadius
            );
            UnityEditor.Handles.DrawLine(
                centerWorld,
                centerWorld + _parentCanvas.up * _maxRadius
            );
            
            // Подпись с координатами
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.alignment = TextAnchor.MiddleCenter;
            
            UnityEditor.Handles.Label(
                centerWorld + Vector3.up * (_maxRadius + 20f),
                $"Center Local: {_centerLocalPosition.ToString("F0")}",
                style
            );
            
            UnityEditor.Handles.Label(
                centerWorld + Vector3.down * (_maxRadius + 30f),
                $"Max Radius: {_maxRadius}",
                style
            );
            
            // Рисуем вектор от центра к текущей позиции мыши (в редакторе)
            if (Application.isPlaying) return;
            
            // Vector2 mouseScreenPos = UnityEditor.Handles.GetMainGameViewMousePosition();
            // if (mouseScreenPos != Vector2.zero)
            // {
            //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //         _parentCanvas,
            //         mouseScreenPos,
            //         null,
            //         out Vector2 mouseLocalPos
            //     );
            //     
            //     Vector2 mouseDir = mouseLocalPos - _centerLocalPosition;
            //     Vector3 mouseWorldStart = _parentCanvas.TransformPoint(_centerLocalPosition);
            //     Vector3 mouseWorldEnd = _parentCanvas.TransformPoint(_centerLocalPosition + mouseDir);
            //     
            //     Gizmos.color = Color.magenta;
            //     Gizmos.DrawLine(mouseWorldStart, mouseWorldEnd);
            //     
            //     float mouseDistance = mouseDir.magnitude;
            //     UnityEditor.Handles.Label(
            //         mouseWorldEnd,
            //         $"Distance: {mouseDistance:F0}",
            //         style
            //     );
            // }
        }
        
        [ContextMenu("Debug Current Center")]
        private void DebugCurrentCenter()
        {
            Debug.Log("=== Joystick Debug Info ===");
            Debug.Log($"Manual Center: {_useManualCenter}");
            Debug.Log($"Center Local Position: {_centerLocalPosition}");
            Debug.Log($"Background anchoredPosition: {_background.anchoredPosition}");
            Debug.Log($"Background localPosition: {_background.localPosition}");
            Debug.Log($"Background pivot: {_background.pivot}");
            Debug.Log($"Background rect size: {_background.rect.size}");
            Debug.Log($"Parent canvas: {_parentCanvas?.name}");
            Debug.Log($"Max Radius: {_maxRadius}");
            
            // Рассчитываем мировую позицию центра
            if (_parentCanvas != null)
            {
                Vector3 worldPos = _parentCanvas.TransformPoint(_centerLocalPosition);
                Debug.Log($"Center World Position: {worldPos}");
            }
        }
        
        [ContextMenu("Calculate Auto Center")]
        private void CalculateAutoCenter()
        {
            Vector2 autoCenter = GetAutoCalculatedCenter();
            Debug.Log($"Auto calculated center: {autoCenter}");
            
            // Если хотите автоматически применить
            // _manualCenterLocal = autoCenter;
            // _useManualCenter = true;
        }
        
        // [ContextMenu("Test with Current Mouse Position")]
        // private void TestWithMousePosition()
        // {
        //     Vector2 mouseScreenPos = UnityEditor.Handles.GetMainGameViewMousePosition();
        //     Debug.Log($"Mouse screen position: {mouseScreenPos}");
        //     
        //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //         _parentCanvas,
        //         mouseScreenPos,
        //         null,
        //         out Vector2 localPoint
        //     );
        //     
        //     Debug.Log($"Mouse local position: {localPoint}");
        //     Debug.Log($"Center local position: {_centerLocalPosition}");
        //     
        //     Vector2 direction = localPoint - _centerLocalPosition;
        //     Debug.Log($"Direction: {direction}, Magnitude: {direction.magnitude}");
        // }
        #endif
    }
}
