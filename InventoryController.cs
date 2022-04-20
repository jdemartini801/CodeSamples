using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour
{

    public SlotInfo[] inventorySlots;
    public PlantSlotInfo[] plantSlots;
    public SlotInfo hotbarSlot1, hotbarSlot2, hotbarSlot3, hotbarSlot4, hotbarSlot5;
    public Hand hand;
    public Sprite emptySlot;
    public Transform itemHolder, body;
    public EventSystem eventSystem;
    public int activeSlot = 1;
    public float itemDropSpeed;

    public void AddToPlantInventory(Plant plant)
    {

        foreach (PlantSlotInfo info in plantSlots)
        {

            // First slot available, store the plant's info and break out of the loop
            if (info.plantName.text.Equals(""))
            {
                info.UpdateSlot(plant.plantName, plant.sproutImage);
                Destroy(plant.gameObject);
                break;
            }

        }

    }

    public void AddToInventory(Item item)
    {

        SlotInfo emptySlot = null;

        foreach(SlotInfo info in inventorySlots){

            // Store emptySlot information for first empty slot found if stacking fails
            if (info.itemName.text.Equals("") && emptySlot == null)
            {
                emptySlot = info;
            } else // If it can stack
            if (info.itemName.text.Equals(item.itemName))
            {
                // Convert item amount to an int, add items being added to it, convert back
                info.UpdateSlot(item.itemName, (int.Parse(info.itemAmount.text) + item.itemAmount).ToString(), item.itemImage);

                // Remove from game
                Destroy(item.gameObject);

                // Return so it doesn't go into the empty slot
                return;
            }

        }

        if(emptySlot != null)
        {
            emptySlot.UpdateSlot(item.itemName, item.itemAmount.ToString(), item.itemImage);
            // Remove from game
            Destroy(item.gameObject);
        }

    }

    public void UpdateInventory()
    {
        // Make sure that the player is holding the object from the active slot in their hand
        SetActiveSlot(activeSlot);
    }

    public void InventoryClick(bool leftClick)
    {

        PointerEventData data = new PointerEventData(eventSystem);
        data.position = Input.mousePosition;

        List<RaycastResult> hitUI = new List<RaycastResult>();

        EventSystem.current.RaycastAll(data, hitUI);

        // Left/Right clicking in empty space
        if(hitUI.Count == 0)
        {

            // If there's nothing in your hand, don't drop items
            if (hand.itemName.Equals(""))
            {
                return;
            }

            int itemAmount = 0;

            if (!leftClick)
            {
                // Drop item
                GameObject item = Instantiate(Resources.Load<GameObject>(hand.itemName), this.transform.position + new Vector3(0, 1.5f, 0) + body.transform.forward, Quaternion.identity);

                // Remove from hand/remove one from hand
                itemAmount = int.Parse(hand.itemAmount.text);
                itemAmount--;
                hand.itemAmount.text = itemAmount.ToString();

                // If empty, remove from hand
                if (itemAmount == 0)
                {
                    hand.UpdateHand("", "0", emptySlot);
                }

            } else
            {

                // Drop item
                Item itemStack = Instantiate(Resources.Load<GameObject>(hand.itemName), this.transform.position + new Vector3(0, 1.5f, 0) + body.transform.forward, Quaternion.identity).GetComponent<Item>();
                itemStack.itemAmount = int.Parse(hand.itemAmount.text);

                hand.UpdateHand("", "0", emptySlot);

            }


        } else
        {

            if (!leftClick)
            {
                foreach (RaycastResult result in hitUI)
                {

                    SlotInfo info = result.gameObject.GetComponent<SlotInfo>();

                    // Right clicked a slot
                    if (info != null)
                    {

                        bool slotEmpty = info.itemName.text.Equals("");
                        int slotAmount = int.Parse(info.itemAmount.text);

                        // Nothing in your hand
                        if (hand.itemName.Equals(""))
                        {

                            if (!slotEmpty)
                            {

                                // Put half of slot in your hand
                                int halfSlot = (int) Mathf.Ceil((float) slotAmount / 2);
                                hand.UpdateHand(info.itemName.text, halfSlot.ToString(), info.itemImage.sprite);

                                int newSlotAmount = slotAmount - halfSlot;
                                
                                if(newSlotAmount > 0)
                                {
                                    info.UpdateSlot(info.itemName.text, newSlotAmount.ToString(), info.itemImage.sprite);
                                } else
                                {
                                    info.UpdateSlot("", "0", emptySlot);
                                }

                            }

                        }
                        else // Something in your hand, see if it matches the slot you're clicking
                        {

                            // Matches, drop one into the stack
                            if (hand.itemName.Equals(info.itemName.text))
                            {

                                info.UpdateSlot(info.itemName.text, (slotAmount + 1).ToString(), info.itemImage.sprite);

                                int newHandAmount = int.Parse(hand.itemAmount.text) - 1;
                                if(newHandAmount > 0)
                                {
                                    // Remove one from hand
                                    hand.UpdateHand(hand.itemName, newHandAmount.ToString(), hand.itemImage.sprite);
                                } else
                                {
                                    // Otherwise, clear hand
                                    hand.UpdateHand("", "0", emptySlot);
                                }

                            }
                            else // Does not match
                            {

                                if (slotEmpty)
                                {
                                    // It it empty? If so just drop one
                                    info.UpdateSlot(hand.itemName, 1.ToString(), hand.itemImage.sprite);

                                    int newHandAmount = int.Parse(hand.itemAmount.text) - 1;
                                    if (newHandAmount > 0)
                                    {
                                        // Remove one from hand
                                        hand.UpdateHand(hand.itemName, newHandAmount.ToString(), hand.itemImage.sprite);
                                    }
                                    else
                                    {
                                        // Otherwise, clear hand
                                        hand.UpdateHand("", "0", emptySlot);
                                    }

                                }

                            }

                        }

                    }

                }
            }
            
        }
        

    }

    public void SetActiveSlot(int slot)
    {

        activeSlot = slot;

        // Delete object in hand

        Transform[] itemsInHand = itemHolder.GetComponentsInChildren<Transform>();
        if(itemsInHand.Length > 1)
        {

            GameObject inHand = itemsInHand[1].gameObject;
            Destroy(inHand);

        }

        string itemName = null;

        if (activeSlot == 1)
        {
            if(!hotbarSlot1.itemName.Equals(""))
            {
                itemName = hotbarSlot1.itemName.text;
            }
        }
        else
        if (activeSlot == 2)
        {
            if (!hotbarSlot2.itemName.Equals(""))
            {
                itemName = hotbarSlot2.itemName.text;
            }
        }
        else
        if (activeSlot == 3)
        {
            if (!hotbarSlot3.itemName.Equals(""))
            {
                itemName = hotbarSlot3.itemName.text;
            }
        }
        else
        if (activeSlot == 4)
        {
            if (!hotbarSlot4.itemName.Equals(""))
            {
                itemName = hotbarSlot4.itemName.text;
            }
        }
        else
        if (activeSlot == 5)
        {
            if (!hotbarSlot5.itemName.Equals(""))
            {
                itemName = hotbarSlot5.itemName.text;
            }
        }

        if(!itemName.Equals(""))
        {
            GameObject reference = Resources.Load<GameObject>(itemName);
            if(reference != null)
            {
                GameObject newItem = Instantiate(reference, itemHolder);
                newItem.GetComponent<Item>().enabled = false;
                newItem.GetComponent<Rigidbody>().isKinematic = true;
                newItem.transform.localPosition = new Vector3(0, 0, 0);
            } else
            {
                Debug.LogError("This item doesn't exist!");
            }
        }

    }

    public void Forage()
    {

        Collider[] nearbyItems = Physics.OverlapSphere(this.transform.position, 1.5f);

        Item nearestItem = null;
        Plant nearestPlant = null;
        float nearestItemDistance = Mathf.Infinity;
        float nearestPlantDistance = Mathf.Infinity;

        foreach (Collider col in nearbyItems){

            Item item = col.GetComponent<Item>();
            Plant plant = col.GetComponent<Plant>();
            if(item != null && item.enabled)
            {

                float dist = Vector3.Distance(col.transform.position, transform.position);
                // New closest object
                if (dist < nearestItemDistance)
                {
                    nearestItemDistance = dist;
                    nearestItem = item;
                }

            } else
            if(plant != null)
            {

                float dist = Vector3.Distance(col.transform.position, transform.position);
                // New closest object
                if (dist < nearestPlantDistance)
                {
                    nearestPlantDistance = dist;
                    nearestPlant = plant;
                }

            }



        }

        // Found something
        if (nearestItem != null)
        {
            AddToInventory(nearestItem);
        }

        if(nearestPlant != null)
        {
            AddToPlantInventory(nearestPlant);
        }

    }

    // This function is called whenever a slot is left clicked on
    public void RegisterSlot(SlotInfo slot)
    {

        // Four cases

        // 1. Click a slot that is empty with nothing in your hand -> Do nothing
        // 2. Click a slot that has something in it while nothing is in your hand -> Put it in your hand
        // 3. Click a slot that has something in it while something is in your hand -> Swap hand and slot
        // 4. Click a slot that has nothing in it while something is in your hand -> Put hand in slot, empty hand

        bool slotEmpty = slot.itemName.text.Equals("");
        bool handEmpty = hand.itemName.Equals("");

        // Case 1 -> Do nothing
        if (slotEmpty && handEmpty)
        {
            return;
        }

        // Case 2 -> Put it in your hand
        if (!slotEmpty && handEmpty)
        {

            hand.UpdateHand(slot.itemName.text, slot.itemAmount.text, slot.itemImage.sprite);
            slot.UpdateSlot("", "0", emptySlot);

        }

        // Case 3 -> Swap hand and slot, unless it's same item!! if its same item, stack

        if(!slotEmpty && !handEmpty)
        {

            // Same item? Stack
            if (slot.itemName.text.Equals(hand.itemName))
            {
                slot.UpdateSlot(slot.itemName.text, (int.Parse(slot.itemAmount.text) + int.Parse(hand.itemAmount.text)).ToString(), slot.itemImage.sprite);

                // Clear hand
                hand.UpdateHand("", "0", emptySlot);

            } else // Otherwise, swap
            {
                string tempName = slot.itemName.text;
                string tempAmount = slot.itemAmount.text;
                Sprite tempSprite = slot.itemImage.sprite;

                slot.UpdateSlot(hand.itemName, hand.itemAmount.text, hand.itemImage.sprite);
                hand.UpdateHand(tempName, tempAmount, tempSprite);

            }

        }

        // Case 4 -> Put hand in slot, empty hand
        if(slotEmpty && !handEmpty)
        {
            slot.UpdateSlot(hand.itemName, hand.itemAmount.text, hand.itemImage.sprite);
            hand.UpdateHand("", "0", emptySlot);
        }

    }

}
