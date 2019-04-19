using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDestination : MonoBehaviour
{

    [SerializeField] CustomNavMeshAgent agent;  

	
	// Update is called once per frame
	void Start ()
    {
        StartCoroutine(DestinationSetter()); 
	}

    IEnumerator DestinationSetter()
    {
        while (true)
        {
            RaycastHit _hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hitInfo))
            {
                agent.SetDestination(_hitInfo.point);
            }
            yield return new WaitForSeconds(.1f); 
        }
    }
}
