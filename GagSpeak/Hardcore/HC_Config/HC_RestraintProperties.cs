using System;
using Newtonsoft.Json.Linq;

namespace GagSpeak.Hardcore;
public class HC_RestraintProperties
{    
    public bool _legsRestraintedProperty { get; set; }        // (Any action which typically involves fast leg movement is restricted)
    public bool _armsRestraintedProperty { get; set; }        // (Any action which typically involves fast arm movement is restricted)
    public bool _gaggedProperty { get; set; }                 // (Any action requiring speech is restricted)
    public bool _blindfoldedProperty { get; set; }            // (Any actions requiring awareness or sight is restricted)
    public bool _immobileProperty { get; set; }               // (Player becomes unable to move in this set)
    public bool _weightyProperty { get; set; }                // (Player is forced to only walk while wearing this restraint)
    public bool _lightStimulationProperty { get; set; }       // (Any action requring focus or concentration has its casttime being slightly slower)
    public bool _mildStimulationProperty { get; set; }        // (Any action requring focus or concentration has its casttime being noticably slower)
    public bool _heavyStimulationProperty { get; set; }       // (Any action requring focus or concentration has its casttime being significantly slower)

    public HC_RestraintProperties() {
        _legsRestraintedProperty = false;
        _armsRestraintedProperty = false;
        _gaggedProperty = false;
        _blindfoldedProperty = false;
        _immobileProperty = false;
        _weightyProperty = false;
        _lightStimulationProperty = false;
        _mildStimulationProperty = false;
        _heavyStimulationProperty = false;
    }

    public bool AnyPropertyTrue()
    {
        return ( 
            _legsRestraintedProperty ||
            _armsRestraintedProperty ||
            _gaggedProperty ||
            _blindfoldedProperty ||
            _immobileProperty ||
            _weightyProperty ||
            _lightStimulationProperty ||
            _mildStimulationProperty ||
            _heavyStimulationProperty
        );
    }
    
    public JObject Serialize() {
        return new JObject() {
            ["LegsRestraintedProperty"] = _legsRestraintedProperty,
            ["ArmsRestraintedProperty"] = _armsRestraintedProperty,
            ["GaggedProperty"] = _gaggedProperty,
            ["BlindfoldedProperty"] = _blindfoldedProperty,
            ["ImmobileProperty"] = _immobileProperty,
            ["WeightyProperty"] = _weightyProperty,
            ["LightStimulationProperty"] = _lightStimulationProperty,
            ["MildStimulationProperty"] = _mildStimulationProperty,
            ["HeavyStimulationProperty"] = _heavyStimulationProperty,
        };
    }

    public void Deserialize(JObject jsonObject) {
        try{
        _legsRestraintedProperty = jsonObject["LegsRestraintedProperty"]?.Value<bool>() ?? false;
        _armsRestraintedProperty = jsonObject["ArmsRestraintedProperty"]?.Value<bool>() ?? false;
        _gaggedProperty = jsonObject["GaggedProperty"]?.Value<bool>() ?? false;
        _blindfoldedProperty = jsonObject["BlindfoldedProperty"]?.Value<bool>() ?? false;
        _immobileProperty = jsonObject["ImmobileProperty"]?.Value<bool>() ?? false;
        _weightyProperty = jsonObject["WeightyProperty"]?.Value<bool>() ?? false;
        _lightStimulationProperty = jsonObject["LightStimulationProperty"]?.Value<bool>() ?? false;
        _mildStimulationProperty = jsonObject["MildStimulationProperty"]?.Value<bool>() ?? false;
        _heavyStimulationProperty = jsonObject["HeavyStimulationProperty"]?.Value<bool>() ?? false;
        } catch (Exception e) {
            GSLogger.LogType.Error($"[HC_RestraintProperties]: Error deserializing HC_RestraintProperties: {e.Message}");
        }
    }
}


