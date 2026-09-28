using UnityEngine;

public enum ZoneType
{
    Checkpoint,
    KillZone
}

[RequireComponent(typeof(Collider))]
public class ZoneTrigger : MonoBehaviour
{
    [SerializeField] private ZoneType zoneType = ZoneType.Checkpoint;

    private void OnTriggerEnter(Collider other)
    {
        // Ищем PlayerRespawn в объекте или его родителях
        PlayerRespawn respawn = other.GetComponentInParent<PlayerRespawn>();
        if (respawn == null) respawn = other.GetComponent<PlayerRespawn>();

        if (respawn == null || respawn.IsDead) return;

        if (zoneType == ZoneType.Checkpoint)
        {
            respawn.SetCheckpoint(transform.position, transform.rotation);
        }
        else if (zoneType == ZoneType.KillZone)
        {
            respawn.DieAndRespawn();
        }
    }
}