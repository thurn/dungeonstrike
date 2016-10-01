using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private Animator _animator;
    private Vector3 _target;
    private GameObject _steeringIndicator;
    private GameObject _nextPositionIndicator;
    private GameObject _destinationIndicator;

    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _destinationIndicator = CreateIndicator(Color.yellow);
        _nextPositionIndicator = CreateIndicator(Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        _destinationIndicator.transform.position = _navMeshAgent.destination;
        _nextPositionIndicator.transform.position = _navMeshAgent.nextPosition;

        if (UnityEngine.Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var cell = GGGrid.GetCellFromRay(ray, 1000f);
            if (cell == null) return;
            _animator.applyRootMotion = true;
            _navMeshAgent.Warp(transform.position);
            _target = cell.CenterPoint3D;
            _animator.SetBool("Walking", true);
            var turn = TurnToTarget(_target);
            Debug.Log("turn: " + turn);
            _animator.SetInteger("Turn", turn);
        }

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
        {
            _animator.applyRootMotion = false;
            _navMeshAgent.SetDestination(_target);
        }

        if (Vector3.Distance(transform.position, _target) < 0.3f)
        {
            _animator.SetBool("Walking", false);
        }
    }

    private GameObject CreateIndicator(Color color)
    {
        var result = GameObject.CreatePrimitive(PrimitiveType.Cube);
        result.transform.localScale = new Vector3(0.1f, 1, 0.1f);
        GameObject.Destroy(result.GetComponent<BoxCollider>());
        result.GetComponent<Renderer>().material.color = color;
        return result;
    }

    private int TurnToTarget(Vector3 target)
    {
        var angle = AngleToTarget(target);
        if (angle < -135) {
            return -180;
        } else if (angle < -45) {
            return -90;
        } else if (angle < 45) {
            return 0;
        } else if (angle < 135) {
            return 90;
        } else {
            return 180;
        }
    }

    private float AngleToTarget(Vector3 target)
    {
        var targetDir = target - transform.position;
		var angle = Vector3.Angle( targetDir, transform.forward );
        Debug.Log("angle: " + angle);

        var targetVector = target - transform.position;
        var rotation = Quaternion.FromToRotation(transform.forward, targetVector);
        var degrees = rotation.eulerAngles.y;
        Debug.Log("degrees: " + degrees);
        return degrees > 180 ? degrees - 360 : degrees;
    }
}
