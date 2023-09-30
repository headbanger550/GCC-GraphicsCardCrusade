using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKLegs : MonoBehaviour
{
    [SerializeField] int chainLenght;

    [Space]

    [SerializeField] Transform target;
    [SerializeField] Transform pole;

    [Space]
    
    [SerializeField] Transform moveTransform;
    [SerializeField] float stepDistance;
    [SerializeField] float moveDuration;
    [SerializeField] float stepOverShoot;

    public bool Moving;

    [Space]

    [SerializeField] int iterations = 10;
    [SerializeField] float delta;

    [Range(0, 1)]
    [SerializeField] float snapBackStrenght;

    protected float[] bonesLenght;
    protected float completeLenght;
    protected Transform[] bones;
    protected Vector3[] positions;
    protected Vector3[] startDirectionSucc;
    protected Quaternion[] startRotationBone;
    protected Quaternion startRotationTarget;
    protected Quaternion startRotationRoot;

    void Awake() 
    {
        Init();
    }

    void Init()
    {
        bones = new Transform[chainLenght + 1];
        positions = new Vector3[chainLenght + 1];
        bonesLenght = new float[chainLenght];
        startDirectionSucc = new Vector3[chainLenght + 1];
        startRotationBone = new Quaternion[chainLenght + 1];

        startRotationTarget = target.rotation;
        completeLenght = 0;

        var current = transform;
        for(var i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotationBone[i] = current.rotation;

            if(i == bones.Length - 1)
            {
                startDirectionSucc[i] = target.position - current.position;
            }
            else
            {
                startDirectionSucc[i] = bones[i + 1].position - current.position;
                bonesLenght[i] = (bones[i + 1].position - current.position).magnitude;
                completeLenght += bonesLenght[i];
            }

            current = current.parent;
        }
    }

    void Update() 
    {   
        //RaycastMovePoint();

        //if(Moving) return;

        //if((transform.position - moveTransform.position).sqrMagnitude >= stepDistance * stepDistance)
        //{
        //    StartCoroutine(MoveToHome());
        //}
    }

    void LateUpdate() 
    {
        ResolveIK();
    }

    void ResolveIK()
    {
        if(target == null)
            return;

        if(bonesLenght.Length != chainLenght)
        {
            Init();
        }

        for(int i = 0; i < bones.Length; i++)
        {
            positions[i] = bones[i].position;
        }

        var rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        var rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);

        if((target.position - bones[0].position).sqrMagnitude >= completeLenght * completeLenght)
        {
            var direction = (target.position - positions[0]).normalized;

            for(int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + direction * bonesLenght[i - 1];
            }
        }
        else
        {
            for(int iteration = 0; iteration < iterations; iteration++)
            {   
                //back
                for(int i = positions.Length - 1; i > 0; i--)
                {
                    if(i == positions.Length - 1)
                    {
                        positions[i] = target.position;
                    }
                    else
                    {
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLenght[i];
                    }
                }

                //forward
                for(int i = 1; i < positions.Length; i++)
                {
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLenght[i - 1];
                }

                if((positions[positions.Length - 1] - target.position).sqrMagnitude < delta * delta)
                    break;
            }
        }

        //move towards pole
        if(pole != null)
        {
            for(int i = 1; i < positions.Length - 1; i++)
            {
                var plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(pole.position);
                var projectedBone = plane.ClosestPointOnPlane(positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        for(int i = 0; i < positions.Length; i++)
        {
            if(i == positions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * startRotationBone[i];
            }
            bones[i].position = positions[i];
        }
        
        for(int i = 0; i < positions.Length; i++)
        {
            bones[i].position = positions[i];
        }
    }

    IEnumerator MoveToHome()
    {
        Moving = true;

        Quaternion startRot = transform.rotation;
        Vector3 startPos = transform.position;

        Quaternion endRot = moveTransform.rotation;
        
        Vector3 towardHome = (moveTransform.position - startPos);

        float overshootDistance = stepDistance * stepOverShoot;
        Vector3 overshootVector = towardHome * overshootDistance;
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        Vector3 endPoint = moveTransform.position + overshootVector;

        Vector3 centerPoint = (startPos + endPoint) / 2;
        centerPoint += moveTransform.up * Vector3.Distance(startPos, endPoint) / 2f;

        float timeElapsed = 0;

        do
        {
            timeElapsed += Time.deltaTime;

            float normalisedTime = timeElapsed / moveDuration;

            transform.position =
                Vector3.Lerp
                (
                    Vector3.Lerp(target.position, centerPoint, normalisedTime),
                    Vector3.Lerp(centerPoint, endPoint, normalisedTime),
                    normalisedTime
                );

            transform.rotation = Quaternion.Slerp(target.rotation, endRot, normalisedTime);

            yield return null;
        }
        while(timeElapsed < moveDuration);
        
        Moving = false;

        target.position = new Vector3(endPoint.x, 0, endPoint.z);
        target.rotation = endRot;
    }

    void RaycastMovePoint()
    {
        RaycastHit _hit;
        if(Physics.Raycast(moveTransform.position, -moveTransform.up, out _hit, Mathf.Infinity))
        {
            moveTransform.position = _hit.point;
        }
    }

}
