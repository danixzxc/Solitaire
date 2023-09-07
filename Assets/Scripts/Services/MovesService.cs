using UniRx;
using Zenject;

namespace Solitaire.Services
{
    public class MovesService : IMovesService
    {
        public IntReactiveProperty Moves { get; private set; } = new IntReactiveProperty();

        [Inject] private readonly IPointsService _points;

        public void Increment()
        {
            Moves.Value += 1;
            _points.Add(-10);
        }

        public void Reset()
        {
            Moves.Value = 0;
        }
    }
}
