using Solitaire.Models;
using Solitaire.Services;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace Solitaire.Presenters
{
    public class PilePresenter : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        [SerializeField] Pile.PileType Type;
        [SerializeField] Pile.CardArrangement Arrangement;
        [SerializeField] Vector3 PosPortrait;
        [SerializeField] Vector3 PosLandscape;

        [Inject] readonly Pile _pile;
        [Inject] readonly Game _game;
        [Inject] readonly IDragAndDropHandler _dndHandler;
        [Inject] readonly OrientationState _orientation;

        public Pile Pile => _pile;

        void Awake()
        {
            _pile.Init(Type, Arrangement, transform.position);
        }

        void Start()
        {
            // Update layout on orientation change
            _orientation.State.Subscribe(UpdateLayout).AddTo(this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData == null || eventData.pointerDrag == null)
            {
                return;
            }

            if (eventData.pointerDrag.TryGetComponent(out CardPresenter cardPresenter) &&
                _pile.CanAddCard(cardPresenter.Card))
            {
                _dndHandler.Drop();
                _game.MoveCard(cardPresenter.Card, _pile);
            }
            else
            {
                _game.PlayErrorSfx();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData?.clickCount == 1 && _pile.IsStock)
            {
                _game.RefillStock();
            }
        }

        private void UpdateLayout(Orientation orientation)
        {
            Vector3 position = orientation == Orientation.Landscape ?
                PosLandscape : PosPortrait;

            transform.position = position;
            _pile.UpdatePosition(position);
        }

        private void Update()
        {
            if (Pile.Cards.Count == 8 && Pile.Type == Pile.PileType.Tableau)

                for (int i = 0; i < Pile.Cards.Count; i++)
                {
                    if (Pile.Cards.ElementAt(i).IsInStack == false || Pile.Cards.ElementAt(i).IsInteractable.Value == false)
                    {
                        break;
                    }
                    if (i == Pile.Cards.Count - 2)
                    {
                        for (int j = 0; j < Pile.Cards.Count; j++)
                        {

                            Pile.Cards.ElementAt(j).Flip();
                            Pile.Cards.ElementAt(j).IsInteractable.Value = false;
                        }
                    }
                }

            for (int i = Pile.Cards.Count - 1; i >= 0; i--)
            {
                if (Pile.Cards.ElementAt(i).IsOnTop) Pile.Cards.ElementAt(i).IsInStack = true;
                else

                {
                    if (Pile.Cards.ElementAt(i).Type == Pile.Cards.ElementAt(i + 1).Type + 1 &&
                        Pile.Cards.ElementAt(i + 1).IsInStack == true &&
                        (((int)Pile.Cards.ElementAt(i).Suit / 2 == 0 && (int)Pile.Cards.ElementAt(i + 1).Suit / 2 == 1) ||
                        ((int)Pile.Cards.ElementAt(i).Suit / 2 == 1 && (int)Pile.Cards.ElementAt(i + 1).Suit / 2 == 0)))
                        Pile.Cards.ElementAt(i).IsInStack = true;
                    else
                    {
                        Pile.Cards.ElementAt(i).IsInStack = false;
                    }
                }
            }
        }
           
    }
}