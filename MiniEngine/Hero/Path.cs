using MiniEngine.BasicComponents;
using OpenTK.Mathematics;

namespace MiniEngine.Hero
{
    internal class Path
    {
        private List<Vector3> _path;

        private Vector3 _source;

        private Vector3 _destination;

        private int _currentNode;

        public Vector3 SourceToDirection { get; private set; }

        public bool IsPathToDo => _path?.Count > 0;

        public Path(List<Vector3> path)
        {
            _path = path;
            if (_path == null || _path.Count == 0) return;

            _source = _path[0];
            _destination = Next();
            SourceToDirection = Vector3.Normalize(_destination - _source);
            _currentNode = 0;
        }

        public void MovingInLines(ref Transformates transformates, float time)
        {
            transformates.Position += SourceToDirection * 1.2f * time;

            Vector3 posDes = Vector3.Normalize(_destination - transformates.Position);

            if (Vector3.Dot(posDes, SourceToDirection) == -1)
            {
                transformates.Position = _destination;
                Vector3 nextnext = NextNext();
                _source = _destination;
                SourceToDirection = Vector3.Normalize(nextnext - _destination);
                _destination = nextnext;
                MoveNext();
            }
        }

        private Vector3 Next() => _path[(_currentNode + 1) % _path.Count];

        private Vector3 NextNext() => _path[(_currentNode + 2) % _path.Count];

        private void MoveNext() => _currentNode = (_currentNode + 1) % _path.Count;
    }
}
