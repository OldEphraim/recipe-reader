using System;
using System.Collections.Generic;
using UnityEngine;
using Board.Input;

public class PieceHandler : MonoBehaviour
{
    public class PieceState
    {
        public int glyphId;
        public int contactId;
        public Vector2 screenPosition;
        public float orientation;
        public bool isActive;
    }

    public event Action<int, Vector2, int> OnPiecePlaced;
    public event Action<int, Vector2, int> OnPieceLifted;
    public event Action<int, Vector2, int> OnPieceMoved;

    private readonly Dictionary<int, PieceState> tracked = new();
    private readonly List<int> scratchRemoveIds = new();

    public IReadOnlyDictionary<int, PieceState> Tracked => tracked;

    void Update()
    {
        var contacts = BoardInput.GetActiveContacts(BoardContactType.Glyph);
        var seen = new HashSet<int>();

        if (contacts != null)
        {
            foreach (var contact in contacts)
            {
                seen.Add(contact.contactId);
                ProcessContact(contact);
            }
        }

        scratchRemoveIds.Clear();
        foreach (var kv in tracked)
        {
            if (!seen.Contains(kv.Key))
            {
                scratchRemoveIds.Add(kv.Key);
            }
        }
        foreach (var id in scratchRemoveIds)
        {
            var state = tracked[id];
            tracked.Remove(id);
            OnPieceLifted?.Invoke(state.glyphId, state.screenPosition, state.contactId);
        }
    }

    private void ProcessContact(BoardContact contact)
    {
        switch (contact.phase)
        {
            case BoardContactPhase.Began:
            {
                var state = new PieceState
                {
                    glyphId = contact.glyphId,
                    contactId = contact.contactId,
                    screenPosition = contact.screenPosition,
                    orientation = contact.orientation,
                    isActive = true
                };
                tracked[contact.contactId] = state;
                OnPiecePlaced?.Invoke(state.glyphId, state.screenPosition, state.contactId);
                break;
            }
            case BoardContactPhase.Moved:
            {
                if (tracked.TryGetValue(contact.contactId, out var state))
                {
                    state.screenPosition = contact.screenPosition;
                    state.orientation = contact.orientation;
                }
                else
                {
                    state = new PieceState
                    {
                        glyphId = contact.glyphId,
                        contactId = contact.contactId,
                        screenPosition = contact.screenPosition,
                        orientation = contact.orientation,
                        isActive = true
                    };
                    tracked[contact.contactId] = state;
                    OnPiecePlaced?.Invoke(state.glyphId, state.screenPosition, state.contactId);
                }
                OnPieceMoved?.Invoke(contact.glyphId, contact.screenPosition, contact.contactId);
                break;
            }
            case BoardContactPhase.Stationary:
            {
                if (!tracked.ContainsKey(contact.contactId))
                {
                    var state = new PieceState
                    {
                        glyphId = contact.glyphId,
                        contactId = contact.contactId,
                        screenPosition = contact.screenPosition,
                        orientation = contact.orientation,
                        isActive = true
                    };
                    tracked[contact.contactId] = state;
                    OnPiecePlaced?.Invoke(state.glyphId, state.screenPosition, state.contactId);
                }
                break;
            }
            case BoardContactPhase.Ended:
            case BoardContactPhase.Canceled:
            {
                if (tracked.TryGetValue(contact.contactId, out var state))
                {
                    tracked.Remove(contact.contactId);
                    OnPieceLifted?.Invoke(state.glyphId, contact.screenPosition, state.contactId);
                }
                break;
            }
        }
    }

    public float GetOrientationDegrees(int contactId)
    {
        if (tracked.TryGetValue(contactId, out var state))
        {
            return state.orientation * Mathf.Rad2Deg;
        }
        return 0f;
    }
}
