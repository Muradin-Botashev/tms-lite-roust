import React, { useState, useEffect } from 'react';
import { Checkbox } from "semantic-ui-react";

const ToggleCheckbox = ({ value, id, disabled, toggleIsActive }) => {

    let [toggle, setToggle] = useState(value);

    useEffect(() => {
        setToggle(value);
    }, [value]);

    useEffect(() => {
        if (toggle !== value) {
            toggleIsActive(id, toggle);
        }
    }, [toggle]);

    return (
        <Checkbox
            toggle
            itemID={id}
            checked={toggle}
            disabled={disabled}
            onChange={(e, data) => {
                e.stopPropagation();
                setToggle(!value);
            }}
        />
    );
};

export default ToggleCheckbox;
