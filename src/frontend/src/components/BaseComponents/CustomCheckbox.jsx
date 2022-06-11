import React from 'react';
import Icon from "../CustomIcon";

const CustomCheckbox = ({checked, onChange, disabled, indeterminate, multi}) => {
    const handleOnChange = (e) => {
        onChange(e)
    };

    let icon = '';
    let color = 'rgba(0, 0, 0, 0.4)';

    if (!multi) {
        if (checked) {
            icon = "checkbox";
            color = "#18a8cc";
        } else {
            icon = "checkbox-empty"
        }
    } else {
        if (checked) {
            icon = "multi-checkbox"
            color = "#18a8cc";
        } else if (indeterminate) {
            icon = "multi-checkbox-some-outline"
            color = "#18a8cc";
        } else {
            icon = "multi-checkbox-empty"
        }
    }



    return (
        <div className={`custom-checkbox ${disabled ? 'custom-checkbox_disabled' : ''}`}>
            <Icon name={icon} size="2"
                  color={color}/>
            <input type="checkbox" checked={checked} disabled={disabled} onClick={handleOnChange}/>
        </div>
    )
};

export default CustomCheckbox;
