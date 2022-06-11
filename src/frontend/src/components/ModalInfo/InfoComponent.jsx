import React, { useState, useMemo } from 'react';
import { Icon } from 'semantic-ui-react';

const InfoComponent = ({ item }) => {
    let [open, setOpen] = useState(false);

    const handleClick = () => {
        setOpen(open => !open);
    };

    const styles = useMemo(
        () => {
            let style = {};
            if (item.messageColumns > 1) {
                let str = '';
                for (let i = 0; i < item.messageColumns; i++) {
                    str = `${str}${i > 0 ? ' ' : ''}1fr`;
                }
                style = {
                    gridTemplateColumns: str,
                    display: 'grid',
                    gridAutoRows: '1.5em',
                    gridGap: '0.5em',
                }
            }

            return style;

        },
        [item.messageColumns],
    );

    return (
        <div className={`info-component info-component__${item.isError ? 'red' : 'green'}`}>
            <div className="info-component_title" onClick={handleClick}>
                <div>
                    <Icon
                        name={item.isError ? 'times circle' : 'check circle'}
                        color={item.isError ? 'red' : 'green'}
                    />
                    <div>{item.title}</div>
                </div>
                <Icon
                    name={open ? 'angle up' : 'angle down'}
                    className="info-component_title_arrow"
                />
            </div>
            <div
                className={`info-component_content ${
                    open ? 'info-component_content__expanded' : ''
                }`}
                style={styles}
            >
                {item.messages.map(message => (
                    <div key={message}>{message}</div>
                ))}
            </div>
        </div>
    );
};

export default InfoComponent;
