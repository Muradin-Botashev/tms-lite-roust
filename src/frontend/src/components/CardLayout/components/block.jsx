import React, { useState, useRef, useEffect } from 'react';
import { Button, Icon, Loader } from 'semantic-ui-react';
import * as Scroll from 'react-scroll/modules';
import { useTranslation } from 'react-i18next';

const Block = ({ item, loading, disabled, actions, isFullScreen, setActiveItem }) => {
    const { t } = useTranslation();

    let [open, setOpen] = useState(true);
    let [full, setFull] = useState(false);
    let [width, setWidth] = useState(0);

    const container = useRef(null);

    const toggleOpen = () => {
        !disabled && setOpen(open => !open);
    };

    const handleFull = () => {
        setFull(full => !full);
    };

    useEffect(() => {
        const width = container && container.current && container.current.offsetWidth;
        setWidth(width);
    }, [container.current, full]);

    useEffect(() => {
        const callBackFunc = () => {
            const top = container.current && container.current.getBoundingClientRect().top;

            if (top < 123 && top > container.current.offsetHeight / 2) {
                setActiveItem(item.menuItem);
            }
        };

        window.addEventListener('scroll', callBackFunc);

        return () => window.removeEventListener('scroll', callBackFunc);
    }, []);

    return (
        <Scroll.Element
            className={`card-content-block ${full ? 'card-content-block__full' : ''}`}
            key={`block-item-${item.menuItem}`}
            name={item.menuItem}
        >
            <Loader active={loading} size="huge">
                Loading
            </Loader>
            <div>
                <div
                    ref={container}
                    className={`card-content-block_header ${
                        disabled ? 'card-content-block_header__disabled' : ''
                    } `}
                >
                    <div className="card-content-block_header_title">
                        <div>{t(item.menuItem)}</div>
                        <div className="card-content-block_header_title__ext">{item.subTitle}</div>
                    </div>
                    <div className="card-content-block_header_actions">
                        {!disabled && actions ? actions().map(item => item) : null}
                        {isFullScreen && (
                            <Button icon={full ? 'compress' : 'expand'} onClick={handleFull} />
                        )}
                        <div
                            className="card-content-block_header_accordion"
                            disabled={disabled}
                            onClick={toggleOpen}
                        >
                            <Icon name={open ? 'angle down' : 'angle up'} />
                        </div>
                    </div>
                </div>
                <div className={`card-content-block_${open ? (full ? 'full' : 'open') : 'close'}`}>
                    {item.render(width, full)}
                </div>
            </div>
        </Scroll.Element>
    );
};

export default Block;
