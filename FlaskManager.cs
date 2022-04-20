/* This script is on every flask in the game. It handles the changing liquid levels and the mixing of their colors. The liquid is a sphere that gets cut off at
the liquid height through a shader. A circular plane is then placed on top of the liquid that has a displacement shader simulating a moving liquid. Flasks can also
be poured out and the liquid transfers to other flasks. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FlaskManager : MonoBehaviour
{
                                                                        // For circle plane displacement
    public float maxLiquidHeight, liquidHeight, topScale, topOffsetY, maxRotationHeight, minHeight, maxHeight, circlePlaneOffset, radialCutoff;
    public float midnightAmount, flareAmount, quixAmount;
    public Transform fluid, circleParent, circlePlane, fluidBase, fluidSide;
    public LayerMask layerMask;
    public Dictionary<Color, float> colors;
    public Color mixedColor;
    public VisualEffect fluidDroplets, bubbles;
    public GameObject fluidSplashPrefab, placeholder;
    public string flaskType;

    private float radius;
    private float seed;
    private float liquidHeightCenter;

    private Material circlePlaneMaterial, fluidMaterial;

    public float temperature;
    public float charge;

    void Start()
    {

        colors = new Dictionary<Color, float>();

        circlePlaneMaterial = circlePlane.GetComponentInChildren<MeshRenderer>().material;
        fluidMaterial = fluid.GetComponent<MeshRenderer>().material;

        seed = Random.Range(1.8f, 2.4f);
        circlePlaneMaterial.SetFloat("_Seed", seed);

    }

    // Update is called once per frame
    void Update()
    {

        // Decrease temperature of flask naturally over time
        if(temperature >= 23f){
            temperature -= .6f * Time.deltaTime;
        }

        // If this potion can bubble, enable bubbles when over boiling point of water
        if(bubbles != null)
        {
            if(temperature >= 100f)
            {
                bubbles.enabled = true;
            } else
            {
                bubbles.enabled = false;
            }
        }

        // Set liquid center height based on liquid height
        liquidHeightCenter = liquidHeight - topOffsetY;

        // Pouring out fluid

        float xRot = this.transform.localEulerAngles.x;
        float zRot = this.transform.localEulerAngles.z;

        // Checks if the potion is tilted to where liquid can fall out
        if((xRot > 90 && xRot < 270) || (zRot > 90 && zRot < 270))
        {

            // Distance from 180
            float xDist = Mathf.Abs(180 - xRot);
            float zDist = Mathf.Abs(180 - zRot);

            float dist;

            if(xDist > zDist)
            {
                dist = xDist;
            } else
            {
                dist = zDist;
            }
            
            // Set flow rate that depends on how tilted the flask is
            float rate = Time.deltaTime * dist * .0003f;
                                               // dampening value

            // Remove fluid from the flask at that time
            AddFluid(mixedColor, -rate, Vector3.zero);

            // If this flask has droplets
            if(fluidDroplets != null)
            {

                // If it's not already active, activate it
                if (liquidHeight > 0 && !fluidDroplets.gameObject.activeSelf)
                {
                    fluidDroplets.gameObject.SetActive(true);
                }

                // If active, raycast down to fill other available flasks
                if (fluidDroplets.gameObject.activeSelf)
                {

                    bool foundFluid = false;

                    // Before we raycast, make sure we're not in the collider of another flask
                    Collider[] fluidColliders = Physics.OverlapSphere(fluidDroplets.transform.position, 0.1f, layerMask);
                    foreach(Collider col in fluidColliders)
                    {
                        if(col.gameObject != fluid.gameObject)
                        {
                            FlaskManager fm = col.transform.GetComponentInParent<FlaskManager>();
                            fm.AddFluid(mixedColor, rate * Mathf.Pow((this.transform.localScale.x / fm.transform.localScale.x), .4f), fluidDroplets.transform.position);
                            foundFluid = true;
                            break;
                        }
                    }

                    // If there was no fluid found from the spherecast, raycast down
                    if (!foundFluid)
                    {
                        Ray fluidRay = new Ray(fluidDroplets.transform.position, -Vector3.up);

                        RaycastHit fluidHit;

                        if (Physics.Raycast(fluidRay, out fluidHit, 20, layerMask))
                        {

                            if (fluidHit.transform != null)
                            {

                                // If it hits a fluid, get the flask manager and add fluid to it at the rate that its being removed from this flask
                                // Take into account the ratio of their scales
                                FlaskManager fm = fluidHit.transform.GetComponentInParent<FlaskManager>();
                                fm.AddFluid(mixedColor, rate * Mathf.Pow((this.transform.localScale.x / fm.transform.localScale.x), .4f), fluidHit.point);

                            }

                        }
                    }

                }


                // If flask is empty, don't show droplets
                if(liquidHeight <= 0 && fluidDroplets.gameObject.activeSelf)
                {
                    fluidDroplets.gameObject.SetActive(false);
                }
            }

        } else {

            if (fluidDroplets != null && fluidDroplets.gameObject.activeSelf)
            {
                fluidDroplets.gameObject.SetActive(false);
            }

        }

        // Set shader variables

        fluidMaterial.SetFloat("_LiquidHeight", liquidHeightCenter);
        fluidMaterial.SetVector("_LiquidBase", fluidBase.transform.position);
        fluidMaterial.SetColor("_LiquidColor", mixedColor);
        circlePlaneMaterial.SetVector("_Center", circlePlane.transform.position);
        circlePlaneMaterial.SetFloat("_Radius", radius);
        circlePlaneMaterial.SetFloat("_MinHeight", minHeight);
        circlePlaneMaterial.SetFloat("_MaxHeight", maxHeight);
        circlePlaneMaterial.SetFloat("_RadialCutoff", radialCutoff);
        circlePlaneMaterial.SetColor("_LiquidColor", mixedColor);
        
        if(bubbles != null)
        {
            bubbles.SetVector4("_BubbleColor", mixedColor);
        }

        if (fluidDroplets != null)
        {
            fluidDroplets.SetVector4("_LiquidColor", mixedColor);
            fluidDroplets.transform.up = Vector3.up;
        }

        // Make sure the top plane of the liquid stays at the center of the flask minus some offset
        circleParent.localPosition = (fluidBase.up * liquidHeightCenter) - (fluidBase.up * circlePlaneOffset);

        
        // This scales the top plane of the liquid to the radius of the flask at that y position
        float distanceFromCenter = Mathf.Abs(circleParent.transform.position.y - fluid.transform.position.y);
        float circleRadius = (fluid.transform.position - fluidSide.transform.position).magnitude;

        radius = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(circleRadius, 2) - Mathf.Pow(distanceFromCenter, 2)));
        radius *= topScale;

        circlePlane.localScale = new Vector3(radius, radius, radius);


        // Make sure the fluid and the fluid base are always pointing straight up regardless of the flask rotation
        if (liquidHeightCenter < maxRotationHeight)
        {
            fluid.transform.up = Vector3.up;
            fluidBase.transform.up = Vector3.up;
        }

        // Set to placeholder position if exists
        if (placeholder != null)
        {
            this.transform.position = placeholder.transform.position;
            this.transform.rotation = placeholder.transform.rotation;
        }

    }

    // Adds specific color fluid to flask and mixes the colors
    public void AddFluid(Color color, float amount, Vector3 point) {

        liquidHeight += amount;
        liquidHeight = Mathf.Clamp(liquidHeight, 0, maxLiquidHeight);

        if(liquidHeight > 0)
        {
            float total = 0;
            foreach (float f in colors.Values)
            {
                total += f;
            }

            // If it's adding, not removing liquid
            if (amount >= 0)
            {

                if (colors.ContainsKey(color))
                {
                    colors[color] += amount;
                }
                else
                {
                    colors.Add(color, amount);
                }

                // Move color of the flask to the mixed color
                mixedColor = Color.Lerp(mixedColor, color, amount / total);

                if (point != Vector3.zero)
                {
                    // Make sure the point is at the top of the liquid, on the circleplane
                    point.y = circlePlane.transform.position.y;

                    // Create splash vfx at the point of contact
                    VisualEffect splash = Instantiate(fluidSplashPrefab, point, fluidBase.transform.rotation).GetComponent<VisualEffect>();
                    splash.SetVector4("_LiquidColor", mixedColor);
                }

            }
            else
            {
                // Remove colors equally from the flask

                List<Color> colorList = new List<Color>();
                foreach (Color key in colors.Keys)
                {
                    colorList.Add(key);
                }

                foreach (Color c in colorList)
                {
                    colors[c] += (colors[c] / total) * amount;
                }

            }
        }
        
    }

    public void SetPlaceholder(GameObject obj)
    {

        // If removing placeholder
        if(obj == null && placeholder != null)
        {
            placeholder.GetComponent<FlaskPlaceholder>().SetHolding(false, null);
        }

        placeholder = obj;

        if(placeholder != null)
        placeholder.GetComponent<FlaskPlaceholder>().SetHolding(true, this);
    }

    public void AddHeat(float amount){
        temperature+=amount;
    }

    public void AddCharge(float amount){
        charge+=amount;
    }

}
