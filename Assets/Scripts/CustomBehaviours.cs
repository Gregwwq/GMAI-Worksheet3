using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using BBUnity.Actions;

public class ShootOnce : GOAction
{
    [InParam("shootPoint")]
    public Transform shootPoint;

    [InParam("bullet")]
    public GameObject bullet;

    [InParam("velocity", DefaultValue = 30f)]
    public float velocity;


    public override void OnStart()
    {
        if (shootPoint == null)
        {
            shootPoint = gameObject.transform.Find("shootPoint");
            if (shootPoint == null)
            {
                Debug.LogWarning("Shoot point not specified. ShootOnce will not work " + "for " + gameObject.name);
            }
        }
        base.OnStart();
    }

    public override TaskStatus OnUpdate()
    {
        if (shootPoint == null)
        {
            return TaskStatus.FAILED;
        }
        GameObject newBullet = GameObject.Instantiate(
                                    bullet,
                                    shootPoint.position,
                                    shootPoint.rotation * bullet.transform.rotation
                                ) as GameObject;
        // Give it a velocity
        if (newBullet.GetComponent<Rigidbody>() == null)
        {
            newBullet.AddComponent<Rigidbody>();
        }
        newBullet.GetComponent<Rigidbody>().velocity = velocity * shootPoint.forward;
        return TaskStatus.COMPLETED;
    }
}

public class Shoot : ShootOnce
{
    [InParam("delay", DefaultValue = 30)]
    public int delay;

    int elap = 0;

    public override TaskStatus OnUpdate()
    {
        if (delay > 0)
        {
            ++elap;
            elap %= delay;
            if (elap != 0) return TaskStatus.RUNNING;
        }
        base.OnUpdate();
        return TaskStatus.RUNNING;
    }
}

public class SleepForever : BasePrimitiveAction
{
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.SUSPENDED;
    }
}

public class IsNightCondition : ConditionBase
{
    private DayNightCycle light;

    public override bool Check()
    {
        if (searchLight())
        {
            return light.isNight;
        }
        else
        {
            return false;
        }
    }

    public override TaskStatus MonitorCompleteWhenTrue()
    {
        if (Check())
        {
            // Light is off. It's night right now.
            return TaskStatus.COMPLETED;
        }
        else
        {
            if (light != null)
            {
                light.OnChanged += OnSunset;
            }
            return TaskStatus.SUSPENDED;
            // We will never awake if light does not exist.
        }
    }

    public override TaskStatus MonitorFailWhenFalse()
    {
        if (!Check())
        {
            // Light does not exist, or is "on" (daylight). Condition is false.
            return TaskStatus.FAILED;
        }
        else
        {
            // Light exists, and is "off" (night). We suspend ourselves
            // until sunrise (when the condition will become false).
            light.OnChanged += OnSunrise;
            return TaskStatus.SUSPENDED;
        }
    }

    public void OnSunset(object sender, System.EventArgs night)
    {
        light.OnChanged -= OnSunset;
        EndMonitorWithSuccess();
    }

    public void OnSunrise(object sender, System.EventArgs e)
    {
        light.OnChanged -= OnSunrise;
        EndMonitorWithFailure();
    }

    public override void OnAbort()
    {
        if (searchLight())
        {
            light.OnChanged -= OnSunrise;
            light.OnChanged -= OnSunset;
        }
        base.OnAbort();
    }

    // Search the global light, and stores in the light field. It returns true
    // if the light was found.
    private bool searchLight()
    {
        if (light != null)
        {
            return true;
        }

        GameObject lightGO = GameObject.FindGameObjectWithTag("MainLight");
        if (lightGO == null)
        {
            return false;
        }

        light = lightGO.GetComponent<DayNightCycle>();
        return light != null;
    } // searchLight

}
