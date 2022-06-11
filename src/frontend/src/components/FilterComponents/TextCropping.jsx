import React, {useEffect, useRef, useState} from 'react';
import {Popup} from 'semantic-ui-react';
import {useSelector} from 'react-redux';
import {representationSelector} from '../../ducks/representations';

const TextCropping = ({children, width: columnWidth, indexColumn}) => {
    const valueRef = useRef(null);
    let [width, setWidth] = useState({
        scrollHeight: 0,
        offsetHeight: 0,
    });

    useEffect(
        () => {
            setWidth({
                scrollHeight: valueRef.current && valueRef.current.scrollHeight,
                offsetHeight: valueRef.current && valueRef.current.offsetHeight,
            });
        },
        [valueRef.current, columnWidth, children],
    );

    return (
        <Popup
            content={children}
            context={valueRef}
            disabled={width.scrollHeight <= width.offsetHeight}
            basic
            position="top center"
            trigger={
                <div className="facet-field__wrap" ref={valueRef}>
                    {children}
                </div>
            }
        />
    );
};

export default TextCropping;
