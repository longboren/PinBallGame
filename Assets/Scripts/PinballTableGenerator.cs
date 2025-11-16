using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PinballTableGenerator : MonoBehaviour
{
    [Header("Table Dimensions")]
    public float tableWidth = 10f;
    public float tableHeight = 15f;
    public float wallHeight = 2f;
    public float wallThickness = 0.5f;

    [Header("Materials")]
    public Material tableMaterial;
    public Material wallMaterial;
    public Material bumperMaterial;
    public Material targetMaterial;
    public Material coinRimMaterial;
    public Material rampMaterial;
    
    [Header("Design Colors (used if Materials are not assigned)")]
    public Color tableColor = new Color(0.9f, 0.85f, 0.8f);
    public Color wallColor = new Color(0.2f, 0.2f, 0.25f);
    public Color rimColor = new Color(0.25f, 0.15f, 0.06f);
    public Color bumperColor = new Color(0.8f, 0.1f, 0.1f);
    public Color coinBodyColor = new Color(0.95f, 0.8f, 0.2f);
    public Color coinRimColor = new Color(0.85f, 0.7f, 0.15f);
    public Color rampColor = new Color(0.5f, 0.35f, 0.2f);
    public Color flipperColor = new Color(0.8f, 0.05f, 0.05f);
    public Color spinnerColor = new Color(0.3f, 0.6f, 0.9f);
    public Color ballColor = new Color(0.95f, 0.95f, 0.95f);

    [Header("Prefabs")]
    public GameObject bumperPrefab;
    public GameObject targetPrefab;
    public GameObject spinnerPrefab;
    public GameObject ballPrefab;
    public GameObject flipperPrefab;

    [Header("Obstacle Settings")]
    public int bumperCount = 5;
    public int targetCount = 8;
    public int spinnerCount = 2;

    private GameObject tableParent;

    void Start()
    {
        // ensure there's a GameManager in scene
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        GenerateTable();
        PlaceObstacles();
        PlaceFlippers();
        SetupDrain();
        SpawnBall();
    }

    void GenerateTable()
    {
        tableParent = new GameObject("PinballTableRoot");
        tableParent.transform.parent = transform;

        // Create table base (use a Plane primitive for a flatter playfield and easier scaling)
        GameObject table = GameObject.CreatePrimitive(PrimitiveType.Plane);
        table.name = "PinballTable";
        table.transform.parent = tableParent.transform;
        // A Unity Plane is 10x10 units by default, so scale accordingly
        table.transform.localScale = new Vector3(tableWidth / 10f, 1f, tableHeight / 10f);
        // Place and tilt slightly so the ball rolls toward the flippers
        table.transform.position = new Vector3(0f, 0f, tableHeight * 0.5f);
        table.transform.rotation = Quaternion.Euler(3f, 0f, 0f); // slight slope

        Renderer tableR = table.GetComponent<Renderer>();
        if (tableR != null)
        {
            if (tableMaterial != null) tableR.material = tableMaterial;
            else tableR.material.color = tableColor;
        }

        // Ensure there's a collider on the table. Plane comes with a MeshCollider; accept that if present.
        Collider tc = table.GetComponent<Collider>();
        if (tc == null) tc = table.AddComponent<BoxCollider>();

        CreateWalls();
        CreateRim();
        CreateRampsAndLanes();
        CreateInvisibleCeiling();
    }

    void CreateRim()
    {
        // Decorative rim around the playfield to improve visuals
        float rimHeight = 0.25f;
        float rimThickness = 0.2f;

        // Left
        GameObject leftRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftRim.name = "Rim_Left";
        leftRim.transform.parent = tableParent.transform;
        leftRim.transform.localScale = new Vector3(rimThickness, rimHeight, tableHeight + 0.5f);
        leftRim.transform.position = new Vector3(-tableWidth * 0.5f - rimThickness * 0.5f, rimHeight * 0.5f, tableHeight * 0.5f);
    Renderer lrR = leftRim.GetComponent<Renderer>();
    if (wallMaterial != null) lrR.material = wallMaterial; else lrR.material.color = rimColor;
        Rigidbody lr = leftRim.AddComponent<Rigidbody>(); lr.isKinematic = true;

        // Right
        GameObject rightRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightRim.name = "Rim_Right";
        rightRim.transform.parent = tableParent.transform;
        rightRim.transform.localScale = new Vector3(rimThickness, rimHeight, tableHeight + 0.5f);
        rightRim.transform.position = new Vector3(tableWidth * 0.5f + rimThickness * 0.5f, rimHeight * 0.5f, tableHeight * 0.5f);
    Renderer rrR = rightRim.GetComponent<Renderer>();
    if (wallMaterial != null) rrR.material = wallMaterial; else rrR.material.color = rimColor;
        Rigidbody rr = rightRim.AddComponent<Rigidbody>(); rr.isKinematic = true;

        // Back
        GameObject backRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backRim.name = "Rim_Back";
        backRim.transform.parent = tableParent.transform;
        backRim.transform.localScale = new Vector3(tableWidth + 0.5f, rimHeight, rimThickness);
        backRim.transform.position = new Vector3(0f, rimHeight * 0.5f, tableHeight + rimThickness * 0.5f);
    Renderer brR = backRim.GetComponent<Renderer>();
    if (wallMaterial != null) brR.material = wallMaterial; else brR.material.color = rimColor;
        Rigidbody br = backRim.AddComponent<Rigidbody>(); br.isKinematic = true;

        // Front rim pieces to frame the flipper gap
        float flipperGap = 2.0f;
        GameObject frontLeftRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontLeftRim.name = "Rim_FrontLeft";
        frontLeftRim.transform.parent = tableParent.transform;
        frontLeftRim.transform.localScale = new Vector3((tableWidth - flipperGap) * 0.5f + 0.25f, rimHeight, rimThickness);
        frontLeftRim.transform.position = new Vector3(- (flipperGap + (tableWidth - flipperGap) * 0.25f), rimHeight * 0.5f, rimThickness * 0.5f - 0.1f);
    Renderer flrR = frontLeftRim.GetComponent<Renderer>();
    if (wallMaterial != null) flrR.material = wallMaterial; else flrR.material.color = rimColor;
        Rigidbody flr = frontLeftRim.AddComponent<Rigidbody>(); flr.isKinematic = true;

        GameObject frontRightRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontRightRim.name = "Rim_FrontRight";
        frontRightRim.transform.parent = tableParent.transform;
        frontRightRim.transform.localScale = new Vector3((tableWidth - flipperGap) * 0.5f + 0.25f, rimHeight, rimThickness);
        frontRightRim.transform.position = new Vector3((flipperGap + (tableWidth - flipperGap) * 0.25f), rimHeight * 0.5f, rimThickness * 0.5f - 0.1f);
    Renderer frrR = frontRightRim.GetComponent<Renderer>();
    if (wallMaterial != null) frrR.material = wallMaterial; else frrR.material.color = rimColor;
        Rigidbody frr = frontRightRim.AddComponent<Rigidbody>(); frr.isKinematic = true;
    }

    void CreateWalls()
    {
        CreateWall("LeftWall", new Vector3(-tableWidth * 0.5f + wallThickness * 0.5f, wallHeight * 0.5f, tableHeight * 0.5f),
                  new Vector3(wallThickness, wallHeight, tableHeight));
        CreateWall("RightWall", new Vector3(tableWidth * 0.5f - wallThickness * 0.5f, wallHeight * 0.5f, tableHeight * 0.5f),
                  new Vector3(wallThickness, wallHeight, tableHeight));
        CreateWall("BackWall", new Vector3(0, wallHeight * 0.5f, tableHeight - wallThickness * 0.5f),
                  new Vector3(tableWidth, wallHeight, wallThickness));

        float flipperGap = 2.0f;
        CreateWall("FrontWallLeft", new Vector3(- (flipperGap + (tableWidth - flipperGap) * 0.25f), wallHeight * 0.5f, wallThickness * 0.5f),
            new Vector3((tableWidth - flipperGap) * 0.5f, wallHeight, wallThickness));
        CreateWall("FrontWallRight", new Vector3((flipperGap + (tableWidth - flipperGap) * 0.25f), wallHeight * 0.5f, wallThickness * 0.5f),
            new Vector3((tableWidth - flipperGap) * 0.5f, wallHeight, wallThickness));
    }

    void CreateWall(string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = tableParent.transform;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.tag = "Wall";

        if (wallMaterial != null)
            wall.GetComponent<Renderer>().material = wallMaterial;
        else
            wall.GetComponent<Renderer>().material.color = wallColor;

        BoxCollider bc = wall.GetComponent<BoxCollider>();
        if (!bc) bc = wall.AddComponent<BoxCollider>();

        if (!wall.GetComponent<Rigidbody>())
        {
            Rigidbody rb = wall.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void CreateRampsAndLanes()
    {
        CreateRamp("LeftRamp", new Vector3(-3f, 0.2f, 8f), new Vector3(2f, 0.1f, 4f), 25f);
        CreateRamp("RightRamp", new Vector3(3f, 0.2f, 10f), new Vector3(2f, 0.1f, 3f), -25f);
        CreateLaneDivider("LeftLaneDivider", new Vector3(-1f, 0.5f, 6f), new Vector3(0.2f, 1f, 8f));
        CreateLaneDivider("RightLaneDivider", new Vector3(1f, 0.5f, 6f), new Vector3(0.2f, 1f, 8f));
    }

    void CreateRamp(string name, Vector3 position, Vector3 scale, float angle)
    {
        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = name;
        ramp.transform.parent = tableParent.transform;
        ramp.transform.position = position;
        ramp.transform.localScale = scale;
        ramp.transform.rotation = Quaternion.Euler(angle, 0, 0);

        if (rampMaterial != null)
            ramp.GetComponent<Renderer>().material = rampMaterial;

        BoxCollider bc = ramp.GetComponent<BoxCollider>();
        if (!bc) bc = ramp.AddComponent<BoxCollider>();

        if (!ramp.GetComponent<Rigidbody>())
        {
            Rigidbody rb = ramp.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    void CreateLaneDivider(string name, Vector3 position, Vector3 scale)
    {
        GameObject divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
        divider.name = name;
        divider.transform.parent = tableParent.transform;
        divider.transform.position = position;
        divider.transform.localScale = scale;
        divider.tag = "Wall";

        BoxCollider bc = divider.GetComponent<BoxCollider>();
        if (!bc) bc = divider.AddComponent<BoxCollider>();

        Rigidbody rb = divider.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void PlaceObstacles()
    {
        // Place bumpers in a small radial pattern for visual balance
        float bumperRadius = Mathf.Min(tableWidth, tableHeight) * 0.2f;
        int count = Mathf.Min(bumperCount, 8); // limit layout
        for (int i = 0; i < count; i++)
        {
            float angle = Mathf.Lerp(20f, 160f, (float)i / Mathf.Max(1, count - 1)) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * bumperRadius;
            float z = tableHeight - Mathf.Sin(angle) * (bumperRadius * 0.6f) - 2f;
            Vector3 pos = new Vector3(x, 1f, z);
            CreateBumper("Bumper_" + i, pos);
        }

        // Arrange targets in a gentle arc near the back of the table
        int targets = Mathf.Max(1, targetCount);
        float arcRadius = tableWidth * 0.35f;
        float arcCenterZ = tableHeight - 2f;
        for (int i = 0; i < targets; i++)
        {
            float t = (float)i / Mathf.Max(1, targets - 1);
            float angle = Mathf.Lerp(-60f, 60f, t) * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * arcRadius;
            float z = arcCenterZ - Mathf.Cos(angle) * 0.5f;
            CreateTarget("Target_" + i, new Vector3(x, 0.6f, z));
        }

    CreateSpinner("LeftSpinner", new Vector3(-tableWidth * 0.25f, 0.6f, tableHeight * 0.45f));
    CreateSpinner("RightSpinner", new Vector3(tableWidth * 0.25f, 0.6f, tableHeight * 0.45f));
    }

    void CreateBumper(string name, Vector3 position)
    {
        GameObject bumper;
        if (bumperPrefab != null)
            bumper = Instantiate(bumperPrefab, position, Quaternion.identity, tableParent.transform);
        else
        {
            bumper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bumper.transform.parent = tableParent.transform;
            bumper.transform.position = position;
            bumper.transform.localScale = Vector3.one * 1.5f;
            Bumper bumperScript = bumper.AddComponent<Bumper>();
            bumperScript.bounceForce = 8f;
            bumperScript.scoreValue = 100;
        }

        bumper.name = name;
        bumper.tag = "Bumper";

        SphereCollider collider = bumper.GetComponent<SphereCollider>();
        if (!collider) collider = bumper.AddComponent<SphereCollider>();
        Rigidbody rb = bumper.GetComponent<Rigidbody>();
        if (!rb) rb = bumper.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // apply bumper material or color
        Renderer rend = bumper.GetComponent<Renderer>();
        if (rend != null)
        {
            if (bumperMaterial != null) rend.material = bumperMaterial;
            else rend.material.color = bumperColor;
        }
    }

    void CreateTarget(string name, Vector3 position)
    {
        GameObject target;
        if (targetPrefab != null)
            target = Instantiate(targetPrefab, position, Quaternion.identity, tableParent.transform);
        else
        {
            // Create a composite coin (parent object) with a body and a slightly larger rim
            target = new GameObject("CoinTargetTemp");
            target.transform.parent = tableParent.transform;
            target.transform.position = position;
            // We'll create two cylinder primitives as visual children
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Coin_Body";
            body.transform.parent = target.transform;
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.45f, 0.03f, 0.45f);
            // Create a rim slightly larger and a bit taller to give edge thickness
            GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rim.name = "Coin_Rim";
            rim.transform.parent = target.transform;
            rim.transform.localPosition = Vector3.zero;
            rim.transform.localScale = new Vector3(0.5f, 0.035f, 0.5f);

            // Rotate whole coin so it lies flat on the playfield
            target.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // Apply materials
            Renderer bodyR = body.GetComponent<Renderer>();
            Renderer rimR = rim.GetComponent<Renderer>();
            if (bodyR != null)
            {
                if (targetMaterial != null) bodyR.material = targetMaterial;
                else
                {
                    bodyR.material.color = coinBodyColor;
                    if (bodyR.material.HasProperty("_Metallic")) bodyR.material.SetFloat("_Metallic", 0.9f);
                    if (bodyR.material.HasProperty("_Glossiness")) bodyR.material.SetFloat("_Glossiness", 0.6f);
                }
            }
            if (rimR != null)
            {
                if (coinRimMaterial != null) rimR.material = coinRimMaterial;
                else if (targetMaterial != null) rimR.material = targetMaterial;
                else rimR.material.color = coinRimColor;
            }

            // Add Target script and coin behaviour to the parent
            Target targetScript = target.AddComponent<Target>();
            targetScript.scoreValue = 50;
            CoinTarget ct = target.AddComponent<CoinTarget>();
            ct.spinSpeed = 120f;

            // Remove the primitive colliders from the visual children to avoid extra physics
            Collider cBody = body.GetComponent<Collider>(); if (cBody) DestroyImmediate(cBody);
            Collider cRim = rim.GetComponent<Collider>(); if (cRim) DestroyImmediate(cRim);
        }

        target.name = name;
        target.tag = "Target";
        Collider collider = target.GetComponent<Collider>();
        if (!collider) collider = target.AddComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    void CreateSpinner(string name, Vector3 position)
    {
        GameObject spinner;
        if (spinnerPrefab != null)
            spinner = Instantiate(spinnerPrefab, position, Quaternion.identity, tableParent.transform);
        else
        {
            spinner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spinner.transform.parent = tableParent.transform;
            spinner.transform.position = position;
            spinner.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
            Spinner spinnerScript = spinner.AddComponent<Spinner>();
            spinnerScript.spinTorque = 50f;
            spinnerScript.reactionForce = 5f;
            spinnerScript.scorePerHit = 10;
        }

        spinner.name = name;
        spinner.tag = "Spinner";
        Rigidbody rb = spinner.GetComponent<Rigidbody>();
        if (!rb) rb = spinner.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        rb.mass = 2f;
        // color the spinner if no material assigned
        Renderer sR = spinner.GetComponent<Renderer>();
        if (sR != null)
        {
            if (spinner.GetComponent<Renderer>() != null)
            {
                if (sR.material != null && sR.material.name == "Default-Diffuse") { /* leave as-is */ }
            }
            sR.material.color = spinnerColor;
        }
    }

    void PlaceFlippers()
    {
        CreateFlipper("LeftFlipper", new Vector3(-1.5f, 0.4f, 1.5f), KeyCode.Z, -45f);
        CreateFlipper("RightFlipper", new Vector3(1.5f, 0.4f, 1.5f), KeyCode.X, 45f);
    }

    void CreateFlipper(string name, Vector3 position, KeyCode key, float initialAngle)
    {
        GameObject flipperRoot = new GameObject(name + "_Pivot");
        flipperRoot.transform.parent = tableParent.transform;
        flipperRoot.transform.position = position;

        GameObject flipper;
        if (flipperPrefab != null)
            flipper = Instantiate(flipperPrefab, position, Quaternion.Euler(0, initialAngle, 0), flipperRoot.transform);
        else
        {
            flipper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flipper.transform.parent = flipperRoot.transform;
            flipper.transform.localPosition = new Vector3(0.6f * Mathf.Sign(initialAngle), 0f, 0f);
            flipper.transform.localScale = new Vector3(1.3f, 0.2f, 0.5f);
            flipper.transform.localRotation = Quaternion.Euler(0, initialAngle, 0);
            Renderer fR = flipper.GetComponent<Renderer>();
            if (fR != null)
            {
                if (fR.material != null) fR.material.color = flipperColor;
                else fR.material.color = flipperColor;
            }
        }

        Rigidbody rb = flipper.GetComponent<Rigidbody>();
        if (!rb) rb = flipper.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.angularDrag = 5f;

        HingeJoint hinge = flipperRoot.AddComponent<HingeJoint>();
        hinge.connectedBody = rb;
        hinge.anchor = Vector3.zero;
        hinge.axis = Vector3.up;
        hinge.autoConfigureConnectedAnchor = false;
        hinge.connectedAnchor = flipper.transform.localPosition;

        JointLimits limits = new JointLimits();
        limits.min = -20f;
        limits.max = 60f;
        hinge.limits = limits;
        hinge.useLimits = true;

        FlipperController fc = flipperRoot.AddComponent<FlipperController>();
        fc.hinge = hinge;
        fc.flipperKey = key;
        fc.motorStrength = 800f;
        fc.restSpeed = -200f;
        fc.activeSpeed = 1000f;
    }

    void CreateInvisibleCeiling()
    {
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "InvisibleCeiling";
        ceiling.transform.parent = tableParent.transform;
        ceiling.transform.position = new Vector3(0, wallHeight + 2f, tableHeight * 0.5f);
        ceiling.transform.localScale = new Vector3(tableWidth, 0.1f, tableHeight);
        
        // Make it invisible
        Renderer renderer = ceiling.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        // Ensure it has a collider
        BoxCollider bc = ceiling.GetComponent<BoxCollider>();
        if (!bc) bc = ceiling.AddComponent<BoxCollider>();
        
        // Make it static
        Rigidbody rb = ceiling.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    void SetupDrain()
    {
        GameObject drain = new GameObject("Drain");
        drain.transform.parent = tableParent.transform;
        drain.transform.position = new Vector3(0f, -0.5f, 0.6f);
        BoxCollider collider = drain.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(3.5f, 1f, 1.5f);
        drain.tag = "Drain";

        DrainDetector dd = drain.AddComponent<DrainDetector>();
        dd.gameManager = FindObjectOfType<GameManager>();
    }

    void SpawnBall()
    {
        Vector3 spawnPos = new Vector3(0f, 1f, tableHeight * 0.8f);
        GameObject ball;
        
        if (ballPrefab != null)
            ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        else
        {
            ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.transform.position = spawnPos;
            ball.transform.localScale = Vector3.one * 0.3f;
            Rigidbody rb = ball.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            // color the ball
            Renderer bR = ball.GetComponent<Renderer>();
            if (bR != null) bR.material.color = ballColor;
        }
        
        ball.tag = "Ball";
        ball.name = "Ball";
        
        // Register with GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.RegisterBall(ball);
        
        // Setup camera to follow ball
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null)
            {
                camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
                camFollow.offset = new Vector3(0, 10, -6);
                camFollow.smoothSpeed = 5f;
            }
            camFollow.target = ball.transform;
        }
    }
}